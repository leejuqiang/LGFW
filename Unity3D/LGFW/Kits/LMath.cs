using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Math kit set
    /// </summary>
    public class LMath
    {

        /// <summary>
        /// Compute the factor based on the current value from a lerp
        /// </summary>
        /// <returns>The factor</returns>
        /// <param name="fromValue">From value</param>
        /// <param name="to">To value</param>
        /// <param name="current">The interpolated value</param>
        public static float lerpValue(float fromValue, float to, float current)
        {
            if (fromValue == to)
            {
                return 0;
            }
            return (current - fromValue) / (to - fromValue);
        }

        /// <summary>
        /// Clamps a vector2's x and y
        /// </summary>
        /// <param name="v">The vector2</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The max value</param>
        public static void clampVector(ref Vector2 v, float min, float max)
        {
            v.x = Mathf.Clamp(v.x, min, max);
            v.y = Mathf.Clamp(v.y, min, max);
        }

        /// <summary>
        /// Clamps a vector3's x and y and z
        /// </summary>
        /// <param name="v">The vector3</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The max value</param>
        public static void clampVector(ref Vector3 v, float min, float max)
        {
            v.x = Mathf.Clamp(v.x, min, max);
            v.y = Mathf.Clamp(v.y, min, max);
            v.z = Mathf.Clamp(v.z, min, max);
        }

        /// <summary>
        /// Clamps a vector4's x and y and z and w
        /// </summary>
        /// <param name="v">The vector4</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The max value</param>
        public static void clampVector(ref Vector4 v, float min, float max)
        {
            v.x = Mathf.Clamp(v.x, min, max);
            v.y = Mathf.Clamp(v.y, min, max);
            v.z = Mathf.Clamp(v.z, min, max);
            v.w = Mathf.Clamp(v.w, min, max);
        }

        /// <summary>
        /// Clamps a vector, each dimension will be clamped by the min and max's dimension correspondingly
        /// </summary>
        /// <param name="v">The vector</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The max value</param>
        public static void clampVector(ref Vector2 v, Vector2 min, Vector2 max)
        {
            v.x = Mathf.Clamp(v.x, min.x, max.x);
            v.y = Mathf.Clamp(v.y, min.y, max.y);
        }

        /// <summary>
        /// Clamps a vector, each dimension will be clamped by the min and max's dimension correspondingly
        /// </summary>
        /// <param name="v">The vector</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The max value</param>
        public static void clampVector(ref Vector3 v, Vector3 min, Vector3 max)
        {
            v.x = Mathf.Clamp(v.x, min.x, max.x);
            v.y = Mathf.Clamp(v.y, min.y, max.y);
            v.z = Mathf.Clamp(v.z, min.z, max.z);
        }

        /// <summary>
        /// Clamps a vector, each dimension will be clamped by the min and max's dimension correspondingly
        /// </summary>
        /// <param name="v">The vector</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The max value</param>
        public static void clampVector(ref Vector4 v, Vector4 min, Vector4 max)
        {
            v.x = Mathf.Clamp(v.x, min.x, max.x);
            v.y = Mathf.Clamp(v.y, min.y, max.y);
            v.z = Mathf.Clamp(v.z, min.z, max.z);
            v.w = Mathf.Clamp(v.w, min.w, max.w);
        }

        /// <summary>
        /// Get the rotation from a normalized vector to another normalized vector, if they are reverse, the rotation will use the given axis
        /// </summary>
        /// <returns>The rotation</returns>
        /// <param name="f">The vector from</param>
        /// <param name="t">The vector to</param>
        /// <param name="axis">The axis</param>
        public static Quaternion rotateNormalFromToWithAxis(Vector3 f, Vector3 t, Vector3 axis)
        {
            float c = Vector3.Dot(f, t);
            if (Mathf.Approximately(c, 1))
            {
                return Quaternion.identity;
            }
            if (Mathf.Approximately(c, -1))
            {
                return Quaternion.AngleAxis(180, axis);
            }
            return Quaternion.FromToRotation(f, t);
        }

        /// <summary>
        /// Make a transform looking at a direction, based on its facing direction, if they are reverse, use the given axis
        /// </summary>
        /// <param name="t">The transform</param>
        /// <param name="lookDirectionWorld">The normalized direction looking at, in world space</param>
        /// <param name="faceDirectionLocal">The normalized facing direction in local space</param>
        /// <param name="axis">The axis</param>
        public static void lookAt(Transform t, Vector3 lookDirectionWorld, Vector3 faceDirectionLocal, Vector3 axis)
        {
            faceDirectionLocal = t.TransformDirection(faceDirectionLocal);
            float c = Vector3.Dot(lookDirectionWorld, faceDirectionLocal);
            if (Mathf.Approximately(c, 1))
            {
                return;
            }
            if (Mathf.Approximately(c, -1))
            {
                axis = t.TransformDirection(axis);
                t.rotation = Quaternion.AngleAxis(180, axis) * t.rotation;
                return;
            }
            Quaternion q = Quaternion.FromToRotation(faceDirectionLocal, lookDirectionWorld);
            t.rotation = q * t.rotation;
        }

        /// <summary>
        /// Shuffles an array
        /// </summary>
        /// <param name="array">The array</param>
        /// <param name="times">One time means randomly choose 2 items and swap them</param>
        /// <typeparam name="T">The type of the element of the array</typeparam>
        public static void shuffleArray<T>(T[] array, int times = -1)
        {
            shuffleArray<T>(array, null, times);
        }

        /// <summary>
        /// Shuffles the array with a randomizer
        /// </summary>
        /// <param name="array">The array</param>
        /// <param name="random">The randomizer</param>
        /// <param name="times">One time means randomly choose 2 items and swap them</param>
        /// <typeparam name="T">The type of the element of the array</typeparam>
        public static void shuffleArray<T>(T[] array, Randomizer random, int times = -1)
        {
            if (times < 0)
            {
                times = array.Length >> 1;
            }
            if (random != null)
            {
                random.FastMode = true;
                for (int i = 0; i < times; ++i)
                {
                    int r1 = random.range(0, array.Length);
                    int r2 = random.range(0, array.Length);
                    T temp = array[r1];
                    array[r1] = array[r2];
                    array[r2] = temp;
                }
                random.FastMode = false;
            }
            else
            {
                for (int i = 0; i < times; ++i)
                {
                    int r1 = Random.Range(0, array.Length);
                    int r2 = Random.Range(0, array.Length);
                    T temp = array[r1];
                    array[r1] = array[r2];
                    array[r2] = temp;
                }
            }
        }

        /// <summary>
        /// Shuffles a list
        /// </summary>
        /// <param name="l">The List</param>
        /// <param name="times">One time means randomly choose 2 items and swap them</param>
        /// <typeparam name="T">The type of the element of the List</typeparam>
        public static void shuffleList<T>(List<T> l, int times = -1)
        {
            shuffleList<T>(l, null, times);
        }

        /// <summary>
        /// Shuffles a list with a randomizer
        /// </summary>
        /// <param name="l">The List</param>
        /// <param name="random">The randomizer</param>
        /// <param name="times">One time means randomly choose 2 items and swap them</param>
        /// <typeparam name="T">The type of the element of the List</typeparam>
        public static void shuffleList<T>(List<T> l, Randomizer random, int times = -1)
        {
            if (times < 0)
            {
                times = l.Count >> 1;
            }
            if (random != null)
            {
                random.FastMode = true;
                for (int i = 0; i < times; ++i)
                {
                    int r1 = random.range(0, l.Count);
                    int r2 = random.range(0, l.Count);
                    T temp = l[r1];
                    l[r1] = l[r2];
                    l[r2] = temp;
                }
                random.FastMode = false;
            }
            else
            {
                for (int i = 0; i < times; ++i)
                {
                    int r1 = Random.Range(0, l.Count);
                    int r2 = Random.Range(0, l.Count);
                    T temp = l[r1];
                    l[r1] = l[r2];
                    l[r2] = temp;
                }
            }
        }


        /// <summary>
        /// Randomly picks some values from an array. Each element is only picked once.
        /// </summary>
        /// <param name="l">The array</param>
        /// <param name="random">The randomizer</param>
        /// <param name="number">The number of elements you want to pick</param>
        /// <param name="notChangeArray">If true, the array l won't be changed</param>
        /// <typeparam name="T">The type of the array's element</typeparam>
        /// <returns>The picked elements. If number is greater then the length of the array, this function returns the original array l</returns>
        public static T[] pickInArray<T>(T[] l, Randomizer random, int number, bool notChangeArray)
        {
            if (number >= l.Length)
            {
                return l;
            }
            if (number <= 0)
            {
                return new T[0];
            }
            T[] ret = new T[number];
            T[] temp = l;
            if (notChangeArray)
            {
                temp = new T[l.Length];
                System.Array.Copy(l, temp, l.Length);
            }
            int count = l.Length;
            if (random != null)
            {
                random.FastMode = true;
                for (int i = 0; i < number; ++i)
                {
                    int r = random.range(0, count);
                    ret[i] = temp[r];
                    --count;
                    temp[r] = temp[count];
                }
                random.FastMode = false;
            }
            else
            {
                for (int i = 0; i < number; ++i)
                {
                    int r = Random.Range(0, count);
                    ret[i] = temp[r];
                    --count;
                    temp[r] = temp[count];
                }
            }
            return ret;
        }

        /// <summary>
        /// Randomly picks some values from an array. Each element is only picked once.
        /// </summary>
        /// <param name="l">The array</param>
        /// <param name="number">The number of elements you want to pick</param>
        /// <param name="notChangeArray">If true, the array l won't be changed</param>
        /// <typeparam name="T">The type of the array's element</typeparam>
        /// <returns>The picked elements. If number is greater then the length of the array, this function returns the original array l</returns>
        public static T[] pickInArray<T>(T[] l, int number, bool notChangeArray)
        {
            return pickInArray<T>(l, null, number, notChangeArray);
        }

        /// <summary>
        /// Randomly picks some values from a list. Each element is only picked once.
        /// </summary>
        /// <param name="l">The list</param>
        /// <param name="random">The randomizer</param>
        /// <param name="number">The number of elements you want to pick</param>
        /// <param name="notChangeArray">If true, the list l won't be changed</param>
        /// <typeparam name="T">The type of the list's element</typeparam>
        /// <returns>The picked elements. If number is greater then the length of the list, this function returns l.ToArray()</returns>
        public static T[] pickInList<T>(List<T> l, Randomizer random, int number, bool notChangeList)
        {
            if (number >= l.Count)
            {
                return l.ToArray();
            }
            if (number <= 0)
            {
                return new T[0];
            }
            T[] ret = new T[number];
            List<T> temp = l;
            if (notChangeList)
            {
                temp = new List<T>(l);
            }
            int count = l.Count;
            if (random != null)
            {
                random.FastMode = true;
                for (int i = 0; i < number; ++i)
                {
                    int r = random.range(0, count);
                    ret[i] = temp[r];
                    --count;
                    temp[r] = temp[count];
                }
                random.FastMode = false;
            }
            else
            {
                for (int i = 0; i < number; ++i)
                {
                    int r = Random.Range(0, count);
                    ret[i] = temp[r];
                    --count;
                    temp[r] = temp[count];
                }
            }
            return ret;
        }

        /// <summary>
        /// Randomly picks some values from a list. Each element is only picked once.
        /// </summary>
        /// <param name="l">The list</param>
        /// <param name="number">The number of elements you want to pick</param>
        /// <param name="notChangeArray">If true, the list l won't be changed</param>
        /// <typeparam name="T">The type of the list's element</typeparam>
        /// <returns>The picked elements. If number is greater then the length of the list, this function returns l.ToArray()</returns>
        public static T[] pickInList<T>(List<T> l, int number, bool notChangeList)
        {
            return pickInList<T>(l, null, number, notChangeList);
        }
    }
}