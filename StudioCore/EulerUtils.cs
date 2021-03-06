﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace StudioCore
{
    public static class EulerUtils
    {
        public enum RotSeq
        {
            zyx, zyz, zxy, zxz, yxz, yxy, yzx, yzy, xyz, xyx, xzy, xzx
        };

        static Vector3 twoaxisrot(float r11, float r12, float r21, float r31, float r32)
        {
            Vector3 ret = new Vector3();
            ret.X = (float)Math.Atan2(r11, r12);
            ret.Y = (float)Math.Acos(r21);
            ret.Z = (float)Math.Atan2(r31, r32);
            return ret;
        }

        static Vector3 threeaxisrot(float r11, float r12, float r21, float r31, float r32)
        {
            Vector3 ret = new Vector3();
            ret.X = (float)Math.Atan2(r31, r32);
            ret.Y = (float)Math.Asin(r21);
            ret.Z = (float)Math.Atan2(r11, r12);
            return ret;
        }

        static Vector3 _quaternion2Euler(Quaternion q, RotSeq rotSeq)
        {
            switch (rotSeq)
            {
                case RotSeq.zyx:
                    return threeaxisrot(2 * (q.X * q.Y + q.W * q.Z),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z,
                        -2 * (q.X * q.Z - q.W * q.Y),
                        2 * (q.Y * q.Z + q.W * q.X),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);


                case RotSeq.zyz:
                    return twoaxisrot(2 * (q.Y * q.Z - q.W * q.X),
                        2 * (q.X * q.Z + q.W * q.Y),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z,
                        2 * (q.Y * q.Z + q.W * q.X),
                        -2 * (q.X * q.Z - q.W * q.Y));


                case RotSeq.zxy:
                    return threeaxisrot(-2 * (q.X * q.Y - q.W * q.Z),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z,
                        2 * (q.Y * q.Z + q.W * q.X),
                        -2 * (q.X * q.Z - q.W * q.Y),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);


                case RotSeq.zxz:
                    return twoaxisrot(2 * (q.X * q.Z + q.W * q.Y),
                        -2 * (q.Y * q.Z - q.W * q.X),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z,
                        2 * (q.X * q.Z - q.W * q.Y),
                        2 * (q.Y * q.Z + q.W * q.X));


                case RotSeq.yxz:
                    return threeaxisrot(2 * (q.X * q.Z + q.W * q.Y),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z,
                        -2 * (q.Y * q.Z - q.W * q.X),
                        2 * (q.X * q.Y + q.W * q.Z),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z);

                case RotSeq.yxy:
                    return twoaxisrot(2 * (q.X * q.Y - q.W * q.Z),
                        2 * (q.Y * q.Z + q.W * q.X),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z,
                        2 * (q.X * q.Y + q.W * q.Z),
                        -2 * (q.Y * q.Z - q.W * q.X));


                case RotSeq.yzx:
                    return threeaxisrot(-2 * (q.X * q.Z - q.W * q.Y),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z,
                        2 * (q.X * q.Y + q.W * q.Z),
                        -2 * (q.Y * q.Z - q.W * q.X),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z);


                case RotSeq.yzy:
                    return twoaxisrot(2 * (q.Y * q.Z + q.W * q.X),
                        -2 * (q.X * q.Y - q.W * q.Z),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z,
                        2 * (q.Y * q.Z - q.W * q.X),
                        2 * (q.X * q.Y + q.W * q.Z));


                case RotSeq.xyz:
                    return threeaxisrot(-2 * (q.Y * q.Z - q.W * q.X),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z,
                        2 * (q.X * q.Z + q.W * q.Y),
                        -2 * (q.X * q.Y - q.W * q.Z),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);


                case RotSeq.xyx:
                    return twoaxisrot(2 * (q.X * q.Y + q.W * q.Z),
                        -2 * (q.X * q.Z - q.W * q.Y),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z,
                        2 * (q.X * q.Y - q.W * q.Z),
                        2 * (q.X * q.Z + q.W * q.Y));


                case RotSeq.xzy:
                    return threeaxisrot(2 * (q.Y * q.Z + q.W * q.X),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z,
                        -2 * (q.X * q.Y - q.W * q.Z),
                        2 * (q.X * q.Z + q.W * q.Y),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);


                case RotSeq.xzx:
                    return twoaxisrot(2 * (q.X * q.Z - q.W * q.Y),
                        2 * (q.X * q.Y + q.W * q.Z),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z,
                        2 * (q.X * q.Z + q.W * q.Y),
                        -2 * (q.X * q.Y - q.W * q.Z));

                default:
                    return Vector3.Zero;

            }
        }

        public static System.Numerics.Vector3 quaternion2Euler(Quaternion q, RotSeq rotSeq)
        {
            Vector3 res = _quaternion2Euler(q, rotSeq);
            var result = new System.Numerics.Vector3();
            float test = q.W * q.Z + q.X * q.Y;
            float unit = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
            switch (rotSeq)
            {
                case RotSeq.zyx:
                    result.X = res.X;
                    result.Y = res.Y;
                    result.Z = res.Z;
                    break;

                case RotSeq.zxy:
                    result.X = res.Y;
                    result.Y = res.X;
                    result.Z = res.Z;
                    break;

                case RotSeq.yxz:
                    result.X = res.Z;
                    result.Y = res.X;
                    result.Z = res.Y;
                    break;

                case RotSeq.yzx:
                    result.X = res.X;
                    result.Y = res.Z;
                    result.Z = res.Y;
                    // Handle poles
                    if (test > 0.4995f * unit)
                    {
                        result.X = 0.0f;
                        result.Y = 2.0f * (float)Math.Atan2(q.Y, q.Z);
                        result.Z = 90.0f * Utils.Deg2Rad;
                    }
                    if (test < -0.4995f * unit)
                    {
                        result.X = 0.0f;
                        result.Y = -2.0f * (float)Math.Atan2(q.Y, q.Z);
                        result.Z = -90.0f * Utils.Deg2Rad;
                    }
                    break;

                case RotSeq.xyz:
                    result.X = res.Z;
                    result.Y = res.Y;
                    result.Z = res.X;
                    break;

                case RotSeq.xzy:
                    result.X = res.Y;
                    result.Y = res.Z;
                    result.Z = res.X;
                    break;

                default:
                    return System.Numerics.Vector3.Zero;
            }
            result.X = (result.X <= -180.0f * Utils.Deg2Rad) ? result.X + 360.0f * Utils.Deg2Rad : result.X;
            result.Y = (result.Y <= -180.0f * Utils.Deg2Rad) ? result.Y + 360.0f * Utils.Deg2Rad : result.Y;
            result.Z = (result.Z <= -180.0f * Utils.Deg2Rad) ? result.Z + 360.0f * Utils.Deg2Rad : result.Z;
            return result;
        }
    }
}
