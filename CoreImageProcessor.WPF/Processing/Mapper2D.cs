using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CoreImageProcessor.Processing
{
    internal abstract class Mapper2D<T> where T : struct
    {
        public Mapper2D(byte[] source, int width, int height, EdgeHandling edgeHandling, T? constant)
        {
            if (edgeHandling == EdgeHandling.Constant && !constant.HasValue)
                throw new ArgumentNullException(nameof(constant));

            Width = width;
            Height = height;
            PixelCount = width * height;
            Source = source;
            EdgeHandling = edgeHandling;
            Constant = constant;
        }

        public abstract int Channels { get; }

        public T[] this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (TryGetValue(x, y, out T[] pixel))
                {
                    return pixel;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    throw new IndexOutOfRangeException();

                SetValue(x, y, value);
            }
        }

        public int Width { get; }

        public int Height { get; }

        public int PixelCount { get; }

        public byte[] Source { get; }

        public EdgeHandling EdgeHandling { get; }

        public T? Constant { get; }

        protected abstract T[] GetValue(int x, int y);

        protected abstract void SetValue(int x, int y, T[] value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int x, int y, out T[] value)
        {
            bool invalidX = x < 0 || x >= Width;
            bool invalidY = y < 0 || y >= Height;

            if (invalidX || invalidY)
            {
                switch (EdgeHandling)
                {
                    case EdgeHandling.Constant:
                        value = new T[Channels];

                        for (int i = 0; i < value.Length; ++i)
                            value[i] = Constant!.Value;

                        return true;
                    case EdgeHandling.Extend:
                        if (invalidX)
                            x = x < 0 ? 0 : Width - 1;
                        
                        if (invalidY)
                            y = y < 0 ? 0 : Height - 1;

                        value = GetValue(x, y);
                        return true;
                    case EdgeHandling.Wrap:
                        if (invalidX)
                        {
                            if (x < 0) x = -x;

                            x %= Width;
                        }

                        if (invalidY)
                        {
                            if (y < 0) y = -y;

                            y %= Height;
                        }

                        value = GetValue(x, y);
                        return true;
                    case EdgeHandling.Mirror:
                        int position, times, rest;

                        if (invalidX)
                        {
                            if (x < 0)
                            {
                                ++x;
                                position = 0;
                                times = -(x / Width);
                                rest = -(x % Width);
                            }
                            else
                            {
                                position = 1;
                                times = (x / Width) - 1;
                                rest = x % Width;
                            }

                            x = (position + times) % 2 == 0 ? rest : Width - 1 - rest;
                        }

                        if (invalidY)
                        {
                            if (y < 0)
                            {
                                ++y;
                                position = 0;
                                times = -(y / Height);
                                rest = -(y % Height);
                            }
                            else
                            {
                                position = 1;
                                times = (y / Height) - 1;
                                rest = y % Height;
                            }

                            y = (position + times) % 2 == 0 ? rest : Height - 1 - rest;
                        }

                        value = GetValue(x, y);
                        return true;
                    default:
                        value = Array.Empty<T>();
                        return false;
                }
            }
            else
            {
                value = GetValue(x, y);
                return true;
            }
        }
    }
}
