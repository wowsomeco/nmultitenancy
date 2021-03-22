using System;
using System.Collections.Generic;

namespace MultiTenancy {
  public static class ListExtensions {
    public delegate T Reducer<T, U>(T prev, U current);
    public delegate bool Iterator<T>(T item, int idx);
    public delegate void IteratorWithPointer<T>(T item, int idx, bool first, bool last);

    public static void LoopWithPointer<T>(this IList<T> l, IteratorWithPointer<T> iter) {
      for (int i = 0; i < l.Count; ++i) {
        iter(l[i], i, i == 0, i == l.Count - 1);
      }
    }

    public static void Loop<T>(this IList<T> l, Iterator<T> iter) {
      for (int i = 0; i < l.Count; ++i) {
        // breaks when iter returns false, 
        // otherwise iterate the next one
        if (!iter(l[i], i)) break;
      }
    }

    public static List<TOut> Map<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, TOut> mapper) {
      List<TOut> newList = new List<TOut>();

      foreach (TIn itm in list)
        newList.Add(mapper(itm));

      return newList;
    }

    public static T Fold<T, U>(this IEnumerable<U> l, T initialValue, Reducer<T, U> reducer) {
      T cur = initialValue;
      foreach (U itm in l) {
        cur = reducer(cur, itm);
      }
      return cur;
    }

    public static List<T> Merge<T>(this List<T> list, params T[] items) {
      if (null != items) {
        foreach (T item in items) {
          list.Add(item);
        }
      }

      return list;
    }

    public static List<T> Combine<T>(params IList<T>[] lists) {
      List<T> newList = new List<T>();
      foreach (IList<T> l in lists) {
        newList.AddRange(l);
      }
      return newList;
    }

    public static T Last<T>(this IList<T> l) {
      return l.Count > 0 ? l[l.Count - 1] : default(T);
    }

    public static T RemoveLast<T>(this IList<T> l) where T : class {
      T t = l.Last();
      if (null != t) {
        l.RemoveAt(l.Count - 1);
      }
      return t;
    }

    public static List<T> RemoveWhere<T>(this List<T> l, Predicate<T> p) {
      List<T> found = l.FindAll(p);
      if (found.Count > 0) l.RemoveAll(p);

      return found;
    }

    public static T RemoveSingleWhere<T>(this List<T> l, Predicate<T> p) {
      T found = l.Find(p);
      if (found != null) l.Remove(found);

      return found;
    }

    public static bool IsEmpty<T>(this IList<T> l) {
      return null == l || l.Count == 0;
    }

    public static List<T> RemoveRangeLast<T>(this List<T> l, int count) {
      List<T> removed = new List<T>();
      if (l.IsEmpty()) return removed;

      int i = l.Count - 1;
      while (removed.Count < count) {
        if (l.IsEmpty()) break;

        removed.Insert(0, l[i]);
        l.RemoveAt(i);
        --i;
      }

      return removed;
    }

    public static void AddRange<T>(this IList<T> l, T[] colls, int from, int to) {
      for (int i = from; i < to; ++i) {
        l.Add(colls[i]);
      }
    }

    public static void SafeAdd<T>(this List<T> l, T item) {
      if (l == null) l = new List<T>();
      l.Add(item);
    }

    public static bool HasDuplicate<T>(this IEnumerable<T> source, out T firstDuplicate) {
      if (source == null) {
        throw new ArgumentNullException(nameof(source));
      }

      var checkBuffer = new HashSet<T>();
      foreach (var t in source) {
        if (checkBuffer.Add(t)) {
          continue;
        }

        firstDuplicate = t;
        return true;
      }

      firstDuplicate = default(T);
      return false;
    }
  }
}