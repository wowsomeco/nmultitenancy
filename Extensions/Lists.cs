using System;
using System.Collections.Generic;

namespace MultiTenancy {
  public static class ListExtensions {
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

    public static List<T> Merge<T>(this List<T> list, params T[] items) {
      if (null != items) {
        foreach (T item in items) {
          list.Add(item);
        }
      }

      return list;
    }

    public static List<T> RemoveWhere<T>(this List<T> l, Predicate<T> p) {
      List<T> found = l.FindAll(p);
      if (found.Count > 0) l.RemoveAll(p);

      return found;
    }

    public static bool IsEmpty<T>(this IList<T> l) {
      return null == l || l.Count == 0;
    }

    public static void SafeAdd<T>(this List<T> l, T item) {
      if (l == null) l = new List<T>();
      l.Add(item);
    }
  }
}