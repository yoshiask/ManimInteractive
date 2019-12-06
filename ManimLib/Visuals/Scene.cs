using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ManimLib.Visuals
{
    public class Scene
    {
        #region Properties
        public List<Shapes.Mobject_Shape> Objects {
            get {
                var _objects = new List<IManimElement>(Commands).FindAll((e) => e.GetType() == typeof(Shapes.Mobject_Shape));
                List<Shapes.Mobject_Shape> objects = new List<Shapes.Mobject_Shape>();
                foreach (IManimElement e in _objects)
                {
                    objects.Add((Shapes.Mobject_Shape)e);
                }
                return objects;
            }
        }

        public List<Animations.Animation> Animations {
            get {
                var _animations = new List<IManimElement>(Commands).FindAll((e) => e.GetType() == typeof(Animations.Animation));
                List<Animations.Animation> animations = new List<Animations.Animation>();
                foreach (IManimElement e in _animations)
                {
                    animations.Add((Animations.Animation)e);
                }
                return animations;
            }
        }

        public ObservableCollection<IManimElement> Commands { get; internal set; }

        public Rendering.Camera Camera { get; set; }
        public int StartingAnimationIndex { get; set; }
        public int EndingAnimationIndex { get; set; }
        #endregion

        #region Objects
        public void SetShapeZIndex(string name, int z)
        {
            var shape = GetShape(name);
            if (shape != null)
            {
                Objects.Remove(shape);
                Objects.Insert(z, shape);
            }
        }
        public int GetShapeZIndex(string name)
        {
            return Objects.IndexOf(GetShape(name));
        }
        public void BringToFront(string name)
        {
            SetShapeZIndex(name, Objects.Count - 1);
        }
        public void SendToBack(string name)
        {
            SetShapeZIndex(name, 0);
        }

        public void SetShapeLocation(string name, Point p)
        {
            var shape = GetShape(name);
            if (shape != null)
            {
                var newShape = shape;
                newShape.Location = p;
                SetShape(name, newShape);
            }
        }
        public Point GetShapeLocation(string name)
        {
            var shape = GetShape(name);
            if (shape != null)
            {
                return shape.Location;
            }
            else
            {
                return new Point();
            }
        }

        public void SetShape(string name, Shapes.Mobject_Shape newShape)
        {
            var shape = GetShape(name);
            if (shape != null)
            {
                Objects[Objects.IndexOf(shape)] = newShape;
            }
        }
        public Shapes.Mobject_Shape GetShape(string name)
        {
            return Objects.Find((s) => s.Name == name);
        }
        public void RemoveShape(string name)
        {
            Objects.Remove(GetShape(name));
        }
        public void RemoveShape(Shapes.Mobject_Shape shape)
        {
            Objects.Remove(shape);
        }

        public void Clear()
        {
            Objects.Clear();
            Animations.Clear();
            Commands = new ObservableCollection<IManimElement>();
        }
        #endregion

        #region Animations
        public void AddAnimation(Animations.Animation ani)
        {
            Animations.Add(ani);
        }
        #endregion

        public Scene(Rendering.Camera camera, int startIndex, int endIndex)
        {
            Camera = camera;
            StartingAnimationIndex = startIndex;
            EndingAnimationIndex = endIndex;
        }
    }
}
