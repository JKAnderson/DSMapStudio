﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SoulsFormats;

namespace StudioCore.MsbEditor
{
    /// <summary>
    /// An action that can be performed by the user in the editor that represents
    /// a single atomic editor action that affects the state of the map. Each action
    /// should have enough information to apply the action AND undo the action, as
    /// these actions get pushed to a stack for undo/redo
    /// </summary>
    public abstract class Action
    {
        abstract public void Execute();
        abstract public void Undo();
    }

    public class PropertiesChangedAction : Action
    {
        private class PropertyChange
        {
            public PropertyInfo Property;
            public object OldValue;
            public object NewValue;
            public int ArrayIndex;
        }

        private object ChangedObject;
        private List<PropertyChange> Changes = new List<PropertyChange>();
        private Action<bool> PostExecutionAction = null;

        public PropertiesChangedAction(object changed)
        {
            ChangedObject = changed;
        }

        public PropertiesChangedAction(PropertyInfo prop, object changed, object newval)
        {
            ChangedObject = changed;
            var change = new PropertyChange();
            change.Property = prop;
            change.OldValue = prop.GetValue(ChangedObject);
            change.NewValue = newval;
            change.ArrayIndex = -1;
            Changes.Add(change);
        }

        public PropertiesChangedAction(PropertyInfo prop, int index, object changed, object newval)
        {
            ChangedObject = changed;
            var change = new PropertyChange();
            change.Property = prop;
            if (index != -1 && prop.PropertyType.IsArray)
            {
                Array a = (Array)change.Property.GetValue(ChangedObject);
                change.OldValue = a.GetValue(index);
            }
            else
            {
                change.OldValue = prop.GetValue(ChangedObject);
            }
            change.NewValue = newval;
            change.ArrayIndex = index;
            Changes.Add(change);
        }

        public void AddPropertyChange(PropertyInfo prop, object newval, int index = -1)
        {
            var change = new PropertyChange();
            change.Property = prop;
            if (index != -1 && prop.PropertyType.IsArray)
            {
                Array a = (Array)change.Property.GetValue(ChangedObject);
                change.OldValue = a.GetValue(index);
            }
            else
            {
                change.OldValue = prop.GetValue(ChangedObject);
            }
            change.NewValue = newval;
            change.ArrayIndex = index;
            Changes.Add(change);
        }

        public void SetPostExecutionAction(Action<bool> action)
        {
            PostExecutionAction = action;
        }

        public override void Execute()
        {
            foreach (var change in Changes)
            {
                if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
                {
                    Array a = (Array)change.Property.GetValue(ChangedObject);
                    a.SetValue(change.NewValue, change.ArrayIndex);
                }
                else
                {
                    change.Property.SetValue(ChangedObject, change.NewValue);
                }
            }
            if (PostExecutionAction != null)
            {
                PostExecutionAction.Invoke(false);
            }
        }

        public override void Undo()
        {
            foreach (var change in Changes)
            {
                if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
                {
                    Array a = (Array)change.Property.GetValue(ChangedObject);
                    a.SetValue(change.OldValue, change.ArrayIndex);
                }
                else
                {
                    change.Property.SetValue(ChangedObject, change.OldValue);
                }
            }
            if (PostExecutionAction != null)
            {
                PostExecutionAction.Invoke(true);
            }
        }
    }

    public class CloneMapObjectsAction : Action
    {
        private Universe Universe;
        private Scene.RenderScene Scene;
        private List<MapObject> Clonables = new List<MapObject>();
        private List<MapObject> Clones = new List<MapObject>();
        private List<Map> CloneMaps = new List<Map>();
        private bool SetSelection;

        public CloneMapObjectsAction(Universe univ, Scene.RenderScene scene, List<MapObject> objects, bool setSelection)
        {
            Universe = univ;
            Scene = scene;
            Clonables.AddRange(objects);
            SetSelection = setSelection;
        }

        public override void Execute()
        {
            foreach (var obj in Clonables)
            {
                var m = Universe.GetLoadedMap(obj.MapID);
                if (m != null)
                {
                    var newobj = obj.Clone();
                    newobj.Name = obj.Name + "_1";
                    m.MapObjects.Insert(m.MapObjects.IndexOf(obj) + 1, newobj);
                    if (obj.Parent != null)
                    {
                        int idx = obj.Parent.ChildIndex(obj);
                        obj.Parent.AddChild(newobj, idx);
                    }
                    newobj.UpdateRenderModel();
                    if (newobj.RenderSceneMesh != null)
                    {
                        newobj.RenderSceneMesh.Selectable = new WeakReference<Scene.ISelectable>(newobj);
                    }
                    Clones.Add(newobj);
                    CloneMaps.Add(m);
                }
            }
            if (SetSelection)
            {
                Selection.ClearSelection();
                foreach (var c in Clones)
                {
                    Selection.AddSelection(c);
                }
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Clones.Count(); i++)
            {
                CloneMaps[i].MapObjects.Remove(Clones[i]);
                if (Clones[i] != null)
                {
                    Clones[i].Parent.RemoveChild(Clones[i]);
                }
                if (Clones[i].RenderSceneMesh != null)
                {
                    Clones[i].RenderSceneMesh.AutoRegister = false;
                    Clones[i].RenderSceneMesh.UnregisterWithScene();
                }
            }
            Clones.Clear();
            if (SetSelection)
            {
                Selection.ClearSelection();
                foreach (var c in Clonables)
                {
                    Selection.AddSelection(c);
                }
            }
        }
    }

    public class CloneParamsAction : Action
    {
        private PARAM Param;
        private string ParamString;
        private List<PARAM.Row> Clonables = new List<PARAM.Row>();
        private List<PARAM.Row> Clones = new List<PARAM.Row>();
        private bool SetSelection = false;

        public CloneParamsAction(PARAM param, string pstring, List<PARAM.Row> rows, bool setsel)
        {
            Param = param;
            Clonables.AddRange(rows);
            ParamString = pstring;
            SetSelection = setsel;
        }

        public override void Execute()
        {
            foreach (var row in Clonables)
            {
                var newrow = new PARAM.Row(row);
                newrow.Name = row.Name != null ? row.Name + "_1" : "";
                Param.Rows.Insert(Param.Rows.IndexOf(row) + 1, newrow);
                Clones.Add(newrow);
            }
            if (SetSelection)
            {
                //EditorCommandQueue.AddCommand($@"param/select/{ParamString}/{Clones[0].ID}");
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Clones.Count(); i++)
            {
                Param.Rows.Remove(Clones[i]);
            }
            Clones.Clear();
            if (SetSelection)
            {
            }
        }
    }

    public class CloneFmgsAction : Action
    {
        private FMG Fmg;
        private string FmgString;
        private List<FMG.Entry> Clonables = new List<FMG.Entry>();
        private List<FMG.Entry> Clones = new List<FMG.Entry>();
        private bool SetSelection = false;

        public CloneFmgsAction(FMG fmg, string fstring, List<FMG.Entry> entries, bool setsel)
        {
            Fmg = fmg;
            Clonables.AddRange(entries);
            FmgString = fstring;
            SetSelection = setsel;
        }

        public override void Execute()
        {
            foreach (var entry in Clonables)
            {
                var newentry = new FMG.Entry(0, "");
                newentry.ID = entry.ID;
                newentry.Text = newentry.Text != null ? newentry.Text : "";
                Fmg.Entries.Insert(Fmg.Entries.IndexOf(entry) + 1, newentry);
                Clones.Add(newentry);
            }
            if (SetSelection)
            {
                //EditorCommandQueue.AddCommand($@"param/select/{ParamString}/{Clones[0].ID}");
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Clones.Count(); i++)
            {
                Fmg.Entries.Remove(Clones[i]);
            }
            Clones.Clear();
            if (SetSelection)
            {
            }
        }
    }

    public class DeleteMapObjectsAction : Action
    {
        private Universe Universe;
        private Scene.RenderScene Scene;
        private List<MapObject> Deletables = new List<MapObject>();
        private List<int> RemoveIndices = new List<int>();
        private List<Map> RemoveMaps = new List<Map>();
        private List<MapObject> RemoveParent = new List<MapObject>();
        private List<int> RemoveParentIndex = new List<int>();
        private bool SetSelection;

        public DeleteMapObjectsAction(Universe univ, Scene.RenderScene scene, List<MapObject> objects, bool setSelection)
        {
            Universe = univ;
            Scene = scene;
            Deletables.AddRange(objects);
            SetSelection = setSelection;
        }

        public override void Execute()
        {
            foreach (var obj in Deletables)
            {
                var m = Universe.GetLoadedMap(obj.MapID);
                if (m != null)
                {
                    RemoveMaps.Add(m);
                    RemoveIndices.Add(m.MapObjects.IndexOf(obj));
                    m.MapObjects.RemoveAt(RemoveIndices.Last());
                    obj.RenderSceneMesh.AutoRegister = false;
                    obj.RenderSceneMesh.UnregisterWithScene();
                    RemoveParent.Add(obj.Parent);
                    if (obj.Parent != null)
                    {
                        RemoveParentIndex.Add(obj.Parent.RemoveChild(obj));
                    }
                    else
                    {
                        RemoveParentIndex.Add(-1);
                    }
                }
            }
            if (SetSelection)
            {
                Selection.ClearSelection();
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Deletables.Count(); i++)
            {
                RemoveMaps[i].MapObjects.Insert(RemoveIndices[i], Deletables[i]);
                Deletables[i].RenderSceneMesh.AutoRegister = true;
                Deletables[i].RenderSceneMesh.RegisterWithScene(Scene);
                if (RemoveParent[i] != null)
                {
                    RemoveParent[i].AddChild(Deletables[i], RemoveParentIndex[i]);
                }
            }
            if (SetSelection)
            {
                Selection.ClearSelection();
                foreach (var d in Deletables)
                {
                    Selection.AddSelection(d);
                }
            }
        }
    }

    public class DeleteParamsAction : Action
    {
        private PARAM Param;
        private List<PARAM.Row> Deletables = new List<PARAM.Row>();
        private List<int> RemoveIndices = new List<int>();
        private bool SetSelection = false;

        public DeleteParamsAction(PARAM param, List<PARAM.Row> rows)
        {
            Param = param;
            Deletables.AddRange(rows);
        }

        public override void Execute()
        {
            foreach (var row in Deletables)
            {
                RemoveIndices.Add(Param.Rows.IndexOf(row));
                Param.Rows.RemoveAt(RemoveIndices.Last());
            }
            if (SetSelection)
            {
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Deletables.Count(); i++)
            {
                Param.Rows.Insert(RemoveIndices[i], Deletables[i]);
            }
            if (SetSelection)
            {
            }
        }
    }

    public class CompoundAction : Action
    {
        private List<Action> Actions;

        private Action<bool> PostExecutionAction = null;

        public CompoundAction(List<Action> actions)
        {
            Actions = actions;
        }

        public void SetPostExecutionAction(Action<bool> action)
        {
            PostExecutionAction = action;
        }

        public override void Execute()
        {
            foreach (var act in Actions)
            {
                if (act != null)
                {
                    act.Execute();
                }
            }
            if (PostExecutionAction != null)
            {
                PostExecutionAction.Invoke(false);
            }
        }

        public override void Undo()
        {
            foreach (var act in Actions)
            {
                if (act != null)
                {
                    act.Undo();
                }
            }
            if (PostExecutionAction != null)
            {
                PostExecutionAction.Invoke(true);
            }
        }
    }
}
