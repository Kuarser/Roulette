using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    public sealed class OriginalSpriteInputTarget
    {
        public struct Point { public short X, Y; public Point(short x, short y) { X = x; Y = y; } }
        public uint Flags, ButtonMask;
        public OriginalSpriteAlignment.Rectangle Rectangle, TouchArea;
        public OriginalSpriteInputTarget Parent;
        // Must be supplied in native sibling (+8) order, not draw/creation order.
        public readonly List<OriginalSpriteInputTarget> Children = new List<OriginalSpriteInputTarget>();
        public static bool Contains(OriginalSpriteAlignment.Rectangle r, Point p)
        { return r.Left <= p.X && p.X < r.Right && r.Top <= p.Y && p.Y < r.Bottom; }
        private static OriginalSpriteAlignment.Rectangle Add(OriginalSpriteAlignment.Rectangle a, OriginalSpriteAlignment.Rectangle b)
        {
            return new OriginalSpriteAlignment.Rectangle { Left = unchecked((short)(a.Left + b.Left)),
                Top = unchecked((short)(a.Top + b.Top)), Right = unchecked((short)(a.Right + b.Right)), Bottom = unchecked((short)(a.Bottom + b.Bottom)) };
        }
        public OriginalSpriteInputTarget FindPoint(ref Point local)
        {
            if ((Flags & 1) == 0) return null;
            foreach (var child in Children)
            {
                if (!Contains(Add(child.Rectangle, child.TouchArea), local)) continue;
                var point = new Point(unchecked((short)(local.X - child.Rectangle.Left)), unchecked((short)(local.Y - child.Rectangle.Top)));
                var target = child.FindPoint(ref point);
                if (target == null || (target.Flags & 0x20000) != 0) continue;
                local = point; return target;
            }
            return (Flags & 4) != 0 ? this : null;
        }
        public OriginalSpriteInputTarget FindButton(uint eventType, uint mask)
        {
            if ((Flags & 1) == 0) return null;
            foreach (var child in Children)
            { var target = child.FindButton(eventType, mask); if (target != null) return target; }
            return (eventType == 0x200010 || eventType == 0x200020) && (Flags & 4) != 0 && (ButtonMask & mask) != 0 ? this : null;
        }
        public static OriginalSpriteInputTarget FindWindow(IList<OriginalSpriteInputTarget> windows, ref Point point)
        {
            foreach (var window in windows)
            {
                if (!Contains(window.Rectangle, point)) continue;
                var local = new Point(unchecked((short)(point.X - window.Rectangle.Left)), unchecked((short)(point.Y - window.Rectangle.Top)));
                var target = window.FindPoint(ref local);
                if (target == null || (target.Flags & 0x20000) != 0) continue;
                point = local; return target;
            }
            return null;
        }
        public Point ToLocal(Point screen)
        {
            for (var node = this; node != null; node = node.Parent)
            { screen.X = unchecked((short)(screen.X - node.Rectangle.Left)); screen.Y = unchecked((short)(screen.Y - node.Rectangle.Top)); }
            return screen;
        }
    }
    public static class OriginalSpriteInputTargetValidation
    {
        public static int Validate()
        {
            int checks = 0;
            Action<bool> check = value => { if (!value) throw new Exception("Sprite input target regression " + checks); ++checks; };
            var rectangle = new OriginalSpriteAlignment.Rectangle { Left = 10, Top = 20, Right = 30, Bottom = 40 };
            check(OriginalSpriteInputTarget.Contains(rectangle, new OriginalSpriteInputTarget.Point(10,20)));
            check(!OriginalSpriteInputTarget.Contains(rectangle, new OriginalSpriteInputTarget.Point(30,20)));
            check(!OriginalSpriteInputTarget.Contains(rectangle, new OriginalSpriteInputTarget.Point(10,40)));
            var root = new OriginalSpriteInputTarget { Flags = 1, Rectangle = new OriginalSpriteAlignment.Rectangle { Left=100,Top=200,Right=300,Bottom=400 } };
            var first = new OriginalSpriteInputTarget { Flags = 5, Rectangle = rectangle, Parent=root, ButtonMask=2 };
            var second = new OriginalSpriteInputTarget { Flags = 5, Rectangle = rectangle, Parent=root, ButtonMask=4 };
            root.Children.Add(first);root.Children.Add(second);
            var point = new OriginalSpriteInputTarget.Point(115,225);
            check(OriginalSpriteInputTarget.FindWindow(new[] {root}, ref point)==first && point.X==5 && point.Y==5);
            first.Flags=0;point=new OriginalSpriteInputTarget.Point(15,25);
            check(root.FindPoint(ref point)==second && point.X==5);
            first.Flags=0x20005;point=new OriginalSpriteInputTarget.Point(15,25);
            check(root.FindPoint(ref point)==second);
            root.Flags=0;point=new OriginalSpriteInputTarget.Point(15,25);
            check(root.FindPoint(ref point)==null && point.X==15);root.Flags=1;
            first.Flags=5;first.TouchArea=new OriginalSpriteAlignment.Rectangle {Left=-5,Top=-5,Right=5,Bottom=5};
            point=new OriginalSpriteInputTarget.Point(6,16);check(root.FindPoint(ref point)==first && point.X==-4 && point.Y==-4);
            point=new OriginalSpriteInputTarget.Point(35,25);check(root.FindPoint(ref point)==null && point.X==35);
            root.Flags=5;point=new OriginalSpriteInputTarget.Point(50,50);check(root.FindPoint(ref point)==root);root.Flags=1;
            check(root.FindButton(0x200010,2)==first);
            check(root.FindButton(0x200020,4)==second);
            check(root.FindButton(0x200001,2)==null);
            first.Flags=0x20005;check(root.FindButton(0x200010,2)==first); // Event overload does not apply no-events bit.
            var local=first.ToLocal(new OriginalSpriteInputTarget.Point(115,225));check(local.X==5 && local.Y==5);
            first.Rectangle=new OriginalSpriteAlignment.Rectangle {Left=32767,Top=0,Right=-32767,Bottom=10};
            local=first.ToLocal(new OriginalSpriteInputTarget.Point(-32768,200));check(local.X==-99 && local.Y==0);
            point=new OriginalSpriteInputTarget.Point(300,200);check(OriginalSpriteInputTarget.FindWindow(new[]{root},ref point)==null);
            return checks;
        }
    }
}
