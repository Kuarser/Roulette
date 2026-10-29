using System;
using UnityEngine;
using X = RouletteDeluxe.OriginalBinary80;

namespace RouletteDeluxe
{
    public static class OriginalWheelArithmetic
    {
        public static readonly double Step = X.Divide(X.Divide(X.Integer(360), X.Integer(37)), X.Integer(2)).ToDouble();
        private static readonly X FullStep = X.Multiply(X.FromDouble(Step), X.Integer(2));
        private static readonly X Turn = X.Integer(36000);
        private static readonly double RadiansPerDegree = BitConverter.Int64BitsToDouble(0x3f91df46a2529d39L);
        public static double RestorePhase(int index) { return X.Multiply(FullStep, X.Integer(index % 37)).ToDouble(); }
        public static float RestoreMainAngle(int index)
        { return X.Subtract(Turn, X.Multiply(FullStep, X.Integer(index % 37))).ToFloat(); }
        public static float RenderAngle(double phase) { return X.Subtract(Turn, X.FromDouble(phase)).ToFloat(); }
        public static int CurrentStop(double phase)
        { return X.Divide(X.Add(X.FromDouble(.05d), X.FromDouble(phase)), FullStep).Truncate(); }
        public static void Advance(ref double phase, double amount, out float angle)
        {
            X sum = X.Add(X.FromDouble(phase), X.FromDouble(amount));
            phase = sum.ToDouble(); angle = X.Subtract(Turn, sum).ToFloat();
        }
        public static double Align(double phase)
        {
            X p = X.FromDouble(phase), step = X.FromDouble(Step);
            X aligned = X.Multiply(X.Integer(CurrentStop(phase)), step);
            while (X.Compare(aligned, p) < 0) aligned = X.Add(aligned, step);
            while (X.Compare(aligned, X.Integer(360)) >= 0) aligned = X.Subtract(aligned, X.Integer(360));
            return aligned.ToDouble();
        }
        public static bool Ease(ref float remaining, ref float velocity)
        {
            X dt = X.FromDouble(.042f);
            X time = X.Subtract(X.FromDouble(remaining), dt);
            remaining = time.ToFloat();
            X v = X.Subtract(X.FromDouble(velocity), X.Multiply(dt, X.FromDouble((float)Step)));
            velocity = v.ToFloat();
            if (X.Compare(v, X.Integer(0)) < 0) { remaining = 0; velocity = 0; return true; }
            return X.Compare(time, X.Integer(0)) <= 0;
        }
        public static double BallRadians(int sourceIndex, double phase, bool restore)
        {
            X angle;
            if (restore)
            {
                X tail = X.Add(X.Subtract(Turn, X.FromDouble(phase)), X.FromDouble(Step));
                angle = X.Add(X.Multiply(X.FromDouble(FullStep.ToDouble()), X.Integer(37-sourceIndex)), tail);
            }
            else
            {
                X total = X.Add(X.Multiply(FullStep, X.Integer(sourceIndex-1)), X.Add(X.FromDouble(phase), X.FromDouble(Step)));
                angle = X.Subtract(Turn, total);
            }
            return X.Multiply(angle, X.FromDouble(RadiansPerDegree)).ToDouble();
        }
        public static Vector2 BallCenter(int sourceIndex, double phase, bool restore)
        {
            double radians = BallRadians(sourceIndex, phase, restore);
            return BallTopLeft(radians,472,384,205,24,24)+new Vector2(12,12);
        }
        public static double NudgeBallRadians(double ballAngle)
        { return X.Multiply(X.Subtract(Turn,X.FromDouble(ballAngle)),X.FromDouble(RadiansPerDegree)).ToDouble(); }
        public static Vector2 BallTopLeft(double radians,int centerX,int centerY,double radius,int width,int height)
        {
            double sine, cosine;
            OriginalNativeMath.SinCos(radians, out sine, out cosine);
            int left = X.Subtract(X.Add(X.Integer(centerX), X.Multiply(X.FromDouble(cosine), X.FromDouble(radius))), X.Integer(width/2)).Truncate();
            int top = X.Subtract(X.Add(X.Integer(centerY), X.Multiply(X.FromDouble(sine), X.FromDouble(radius))), X.Integer(height/2)).Truncate();
            return new Vector2(left, top);
        }
    }
}
