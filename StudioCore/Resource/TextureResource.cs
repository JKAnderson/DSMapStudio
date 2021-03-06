﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SoulsFormats;
using Veldrid;

namespace StudioCore.Resource
{
    public class TextureResource : IResource, IDisposable
    {
        public TPF Texture { get; private set; } = null;
        private int TPFIndex = 0;

        public Scene.TexturePool.TextureHandle GPUTexture { get; private set; } = null;

        public TextureResource()
        {
            throw new Exception("Created wrong");
        }

        public TextureResource(TPF tex, int index)
        {
            Texture = tex;
            TPFIndex = index;
        }

        public bool _LoadTexture(AccessLevel al)
        {
            GPUTexture = Scene.Renderer.GlobalTexturePool.AllocateTextureDescriptor();
            if (Texture.Platform == TPF.TPFPlatform.PC)
            {
                Scene.Renderer.AddBackgroundUploadTask((d, cl) =>
                {
                    GPUTexture.FillWithTPF(d, cl, Texture.Platform, Texture.Textures[TPFIndex]);
                    Texture = null;
                });
            }
            else if (Texture.Platform == TPF.TPFPlatform.PS4)
            {
                Scene.Renderer.AddBackgroundUploadTask((d, cl) =>
                {
                    GPUTexture.FillWithPS4TPF(d, cl, Texture.Platform, Texture.Textures[TPFIndex]);
                    Texture = null;
                });
            }
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TextureResource()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        bool IResource._Load(byte[] bytes, AccessLevel al, GameType type)
        {
            return _LoadTexture(al);
        }

        bool IResource._Load(string file, AccessLevel al, GameType type)
        {
            return _LoadTexture(al);
        }
        #endregion
    }
}
