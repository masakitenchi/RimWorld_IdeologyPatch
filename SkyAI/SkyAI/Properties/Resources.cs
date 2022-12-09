using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SkyAI.Properties
{
	// Token: 0x02000002 RID: 2
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		internal Resources()
		{
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x0000205C File Offset: 0x0000025C
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				bool flag = Resources.resourceMan == null;
				if (flag)
				{
					ResourceManager resourceManager = new ResourceManager("SkyAI.Properties.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000003 RID: 3 RVA: 0x000020A4 File Offset: 0x000002A4
		// (set) Token: 0x06000004 RID: 4 RVA: 0x000020BB File Offset: 0x000002BB
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000005 RID: 5 RVA: 0x000020C4 File Offset: 0x000002C4
		internal static byte[] agro
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("agro", Resources.resourceCulture);
				return (byte[])@object;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000006 RID: 6 RVA: 0x000020F4 File Offset: 0x000002F4
		internal static byte[] animalagro
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("animalagro", Resources.resourceCulture);
				return (byte[])@object;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000007 RID: 7 RVA: 0x00002124 File Offset: 0x00000324
		internal static byte[] doctor
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("doctor", Resources.resourceCulture);
				return (byte[])@object;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002154 File Offset: 0x00000354
		internal static byte[] leader
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("leader", Resources.resourceCulture);
				return (byte[])@object;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000009 RID: 9 RVA: 0x00002184 File Offset: 0x00000384
		internal static byte[] lol
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("lol", Resources.resourceCulture);
				return (byte[])@object;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600000A RID: 10 RVA: 0x000021B4 File Offset: 0x000003B4
		internal static byte[] menuLogo
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("menuLogo", Resources.resourceCulture);
				return (byte[])@object;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600000B RID: 11 RVA: 0x000021E4 File Offset: 0x000003E4
		internal static byte[] raidleader
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("raidleader", Resources.resourceCulture);
				return (byte[])@object;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002214 File Offset: 0x00000414
		internal static byte[] trader
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("trader", Resources.resourceCulture);
				return (byte[])@object;
			}
		}

		// Token: 0x04000001 RID: 1
		private static ResourceManager resourceMan;

		// Token: 0x04000002 RID: 2
		private static CultureInfo resourceCulture;
	}
}
