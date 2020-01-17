using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    /// <summary>
    /// Это главный сортировщик в любом дереве, логику по сортировке добавлять сюда
    /// </summary>
    public class FreeHierarchyTreeItemComparer : IComparer<FreeHierarchyTreeItem>
    {
        public int Compare(FreeHierarchyTreeItem x, FreeHierarchyTreeItem y)
        {
            //return 0;

            if (x.HierObject == null && y.HierObject == null) return LogicCompare(x.StringName, y.StringName);
            if (x.HierObject == null) return -1;
            if (y.HierObject == null) return 1;

            //TODO тут нужна пользовательская сортировка, она приоритетней всего

            if (x.SortNumber.HasValue || y.SortNumber.HasValue)
            {
                if (x.SortNumber.GetValueOrDefault(0) > y.SortNumber.GetValueOrDefault(0)) return 1;
                if (x.SortNumber.GetValueOrDefault(0) < y.SortNumber.GetValueOrDefault(0)) return -1;


            }
            //Но это должен быть не простой sortOrder, там надо запоминать положение относительно остальных объектов

            //По типу надо всегда сортировать, более высокий уровень иерархии должен быть сверху
            if ((byte)x.HierObject.Type > (byte)y.HierObject.Type) return 1;
            if ((byte)x.HierObject.Type < (byte)y.HierObject.Type) return -1;

            //Тут нужно дорабатывать
            //var isNameCompared = false;
            //
            //List<KeyValuePair<enumSortOrder, bool>> sortOrders;
            //if (Manager.Config != null && Manager.Config.SortOrder != null && Manager.Config.SortOrder.TryGetValue(enumTypeHierarchy.Info_TI, out sortOrders) && sortOrders != null)
            //{
            //    var propertiesX = ReflectionHelper.SQLTableDefinedTypePropertyDict[x.HierObject.GetType()];

            //    foreach (var sortOrder in sortOrders.Where(s => s.Key != enumSortOrder.None))
            //    {
            //        int r;
            //        if (sortOrder.Key == enumSortOrder.Name)
            //        {
            //            r = LogicCompare(x.HierObject.Name, y.HierObject.Name);

            //            if (r == 0)
            //            {
            //                r = x.HierObject.Id.CompareTo(y.HierObject.Id);
            //            }

            //            if (r != 0)
            //            {
            //                if (!sortOrder.Value) return r * -1;
            //                return r;
            //            }

            //            isNameCompared = true;

            //            continue;
            //        }

            //        var sortName = sortOrder.Key.ToString();
            //        var valueX = ReflectionHelper.ReflectOnPathNullable(x.HierObject, sortName, propertiesX) as IComparable;
            //        var valueY = ReflectionHelper.ReflectOnPathNullable(y.HierObject, sortName, propertiesX) as IComparable; //Предполагаем что типы одинаковые

            //        if (valueY != null && valueX != null)
            //        {
            //            r = valueX.CompareTo(valueY);
            //        }
            //        else if (valueY == null && valueX == null) continue;
            //        else
            //        {
            //            r = valueX != null ? 1 : -1;
            //        }

            //        if (r != 0)
            //        {
            //            return sortOrder.Value ? r : r * -1;
            //        }
            //    }
            //}

            //var c = x.HierObject.Type.CompareTo(y.HierObject.Type);
            //if (c != 0) return c;

            //c = SafeNativeMethods.StrCmpLogicalW(x.HierObject.Name, y.HierObject.Name);

            //if (!isNameCompared)
            {
                var c = LogicCompare(x.HierObject.Name, y.HierObject.Name);

                if (c != 0) return c;
            }
            //Если одинаковые названия
            return x.HierObject.Id.CompareTo(y.HierObject.Id);
        }

        private int LogicCompare(string x, string y)
        {
            //return SafeNativeMethods.StrCmpLogicalW(x, y);

            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lx = x.Length, ly = y.Length;

            for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
            {

                if (char.IsDigit(x[mx]) && char.IsDigit(y[my]))
                {
                    long vx = 0, vy = 0;

                    for (; mx < lx && char.IsDigit(x[mx]); mx++)
                        vx = vx * 10 + x[mx] - '0';

                    for (; my < ly && char.IsDigit(y[my]); my++)
                        vy = vy * 10 + y[my] - '0';

                    if (vx != vy)
                        return vx > vy ? 1 : -1;
                }

                if (mx < lx && my < ly && x[mx] != y[my])
                    return x[mx] > y[my] ? 1 : -1;
            }

            return lx - ly;
        }
    }

    public class NaturalSort : IComparer<object> {

		private string sortColumn;
		private Regex tokenize = new Regex(@"[a-zA-Z]+|[0-9\.-]+", RegexOptions.Compiled);

		public NaturalSort () {
			sortColumn = null;
		}
		public NaturalSort (string newSortColumn) {
			sortColumn = newSortColumn;
		}

		public int Compare (object x, object y) {
			
			string a, b;
			decimal decA, decB;
			DateTime dtA, dtB;
			
			// get the values as strings from the column we want to sort by
			if (sortColumn != null) {
				a = (x as Dictionary<string, object>)[sortColumn].ToString();
				b = (y as Dictionary<string, object>)[sortColumn].ToString();
			} else {
				a = x.ToString();
				b = y.ToString();
			}
			
			// tokenize and sort only if both values are not numeric or valid dates
			if (!decimal.TryParse(a, out decA) &&
				!decimal.TryParse(b, out decB) &&
				!DateTime.TryParse(a, out dtA) &&
				!DateTime.TryParse(b, out dtB)) {
				
				// tokenize on consecutive alphas or valid numbers
				MatchCollection aTok = tokenize.Matches(a);
				MatchCollection bTok = tokenize.Matches(b);

				// attempt to compare each token
				for (int tok = 0; tok < Math.Min(aTok.Count, bTok.Count); tok++) {
					int iTok = CompareToken(aTok[tok].Value, bTok[tok].Value);
					// only retun if find a sortable pair
					if (iTok != 0)
						return iTok;
				}
				
			}
			
			// otherwise compare if a simple value was found
			return CompareToken(a, b);
			
		}
		
		public int CompareToken (string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 0;
            if (string.IsNullOrEmpty(a)) return -1;
            if (string.IsNullOrEmpty(b)) return 1;

            decimal decA, decB;
			bool bDecA = decimal.TryParse(a, out decA), bDecB = decimal.TryParse(b, out decB);
			// attempt numeric comparison assuming decimal < * - ignore left 0-padded numerics
			if ((bDecA || bDecB) && a[0] != '0' && b[0] != '0') {
				if ((bDecA && !bDecB) || (bDecA && bDecB && decA < decB))
					return -1;
				else if ((!bDecA && bDecB) || (bDecA && bDecB && decA > decB))
					return 1;
				else
					return 0;
			}

			// attempt DateTime comparison
			DateTime dtA, dtB;
			// datetime will take precedence if values are different types 
			bool bDtA = DateTime.TryParse(a, out dtA), bDtB = DateTime.TryParse(b, out dtB);
			if (bDtA || bDtB) {
				if ((bDtA && !bDtB) || (bDtA && bDtB && dtA < dtB))
					return -1;
				else if ((!bDtA && bDtB) || (bDtA && bDtB && dtA > dtB))
					return 1;
				else
					return 0;
			}
			
			// otherwise, rely on the default string compare
			return(Comparer<object>.Default.Compare(a, b));
			
		}

	}

    internal static class SafeNativeMethods /// Имплементация на C#
    {

        public static int StrCmpLogicalW (string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 0;
            if (string.IsNullOrEmpty(a)) return -1;
            if (string.IsNullOrEmpty(b)) return 1;

            decimal decA, decB;
            bool bDecA = decimal.TryParse(a, out decA), bDecB = decimal.TryParse(b, out decB);
            // attempt numeric comparison assuming decimal < * - ignore left 0-padded numerics
            if ((bDecA || bDecB) && a[0] != '0' && b[0] != '0') {
                if ((bDecA && !bDecB) || (bDecA && bDecB && decA < decB))
                    return -1;
                else if ((!bDecA && bDecB) || (bDecA && bDecB && decA > decB))
                    return 1;
                else
                    return 0;
            }

            // attempt DateTime comparison
            DateTime dtA, dtB;
            // datetime will take precedence if values are different types 
            bool bDtA = DateTime.TryParse(a, out dtA), bDtB = DateTime.TryParse(b, out dtB);
            if (bDtA || bDtB) {
                if ((bDtA && !bDtB) || (bDtA && bDtB && dtA < dtB))
                    return -1;
                else if ((!bDtA && bDtB) || (bDtA && bDtB && dtA > dtB))
                    return 1;
                else
                    return 0;
            }
			
            // otherwise, rely on the default string compare
            return(Comparer<object>.Default.Compare(a, b));
			
        }
    }


    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods2
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }
}
