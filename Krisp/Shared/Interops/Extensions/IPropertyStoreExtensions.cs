using System;
using System.Runtime.InteropServices;

namespace Shared.Interops.Extensions
{
	public static class IPropertyStoreExtensions
	{
		public static T GetValue<T>(this IPropertyStore propStore, PROPERTYKEY key)
		{
			PropVariant propVariant = default(PropVariant);
			try
			{
				propVariant = propStore.GetValue(ref key);
				VarEnum varType = propVariant.varType;
				if (varType <= VarEnum.VT_LPWSTR)
				{
					if (varType != VarEnum.VT_EMPTY)
					{
						if (varType == VarEnum.VT_LPWSTR)
						{
							return (T)((object)Convert.ChangeType(Marshal.PtrToStringUni(propVariant.pwszVal), typeof(T)));
						}
					}
					else
					{
						if (typeof(T).IsValueType)
						{
							return default(T);
						}
						return (T)((object)Convert.ChangeType(null, typeof(T)));
					}
				}
				else
				{
					if (varType == VarEnum.VT_BLOB)
					{
						return (T)((object)Marshal.PtrToStructure(propVariant.blobData.Data, typeof(T)));
					}
					if (varType == VarEnum.VT_CLSID)
					{
						return (T)((object)Marshal.PtrToStructure(propVariant.pclsidVal, typeof(Guid)));
					}
				}
				throw new NotImplementedException();
			}
			finally
			{
				Ole32.PropVariantClear(ref propVariant);
			}
			T t;
			return t;
		}

		public static void SetValue<T>(this IPropertyStore propStore, PROPERTYKEY key, T value)
		{
			PropVariant propVariant = default(PropVariant);
			try
			{
				if (value is string)
				{
					propVariant.varType = VarEnum.VT_LPWSTR;
					propVariant.pwszVal = Marshal.StringToHGlobalAuto(value as string);
					PropVariant propVariant2 = propVariant;
					propStore.SetValue(ref key, ref propVariant2);
					propStore.Commit();
				}
				else
				{
					if (!(value is bool))
					{
						throw new NotImplementedException();
					}
					propVariant.varType = VarEnum.VT_BOOL;
					propVariant.boolVal = (value as short?).Value;
					PropVariant propVariant2 = propVariant;
					propStore.SetValue(ref key, ref propVariant2);
					propStore.Commit();
				}
			}
			finally
			{
				Ole32.PropVariantClear(ref propVariant);
			}
		}
	}
}
