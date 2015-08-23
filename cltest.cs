using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TCore.CmdLine
{
	public partial class CmdLine
	{
		static void UnitTestNoDispatch()
		{
			CmdLineConfig cfg = new CmdLineConfig(new CmdLineSwitch[] 
				{
					new CmdLineSwitch("t", true, true, "required toggle switch t", null, null),
					new CmdLineSwitch("a", true, false, "optional toggle switch a", null, null),
					new CmdLineSwitch("b", false, false, "optional switch b", null, null),
					new CmdLineSwitch("abc", false, false, "optional multichar switch abc", null, null)
				} );

			CmdLine cl = new CmdLine(cfg);
			string sError;

			Debug.Assert(cl.FParse(new string[] { "-t" }, null, null, out sError));
			cl = new CmdLine(cfg);

			Debug.Assert(cl.FParse(new string[] { "-t", "-a" }, null, null, out sError));
			Debug.Assert(cl.FIsSwitchSet("t"));
			Debug.Assert(cl.FIsSwitchSet("a"));
			Debug.Assert(!cl.FIsSwitchSet("b"));
			Debug.Assert(!cl.FIsSwitchSet("abc"));

			cl = new CmdLine(cfg);

			Debug.Assert(cl.FParse(new string[] { "-t", "-a", "-b", "bar" }, null, null, out sError));
			Debug.Assert(cl.FIsSwitchSet("t"));
			Debug.Assert(cl.FIsSwitchSet("a"));
			Debug.Assert(cl.FIsSwitchSet("b"));
			Debug.Assert(!cl.FIsSwitchSet("abc"));

			Debug.Assert(String.Compare("bar", cl.ParamFromSwitch("b")) == 0);

			cl = new CmdLine(cfg);

			Debug.Assert(cl.FParse(new string[] { "-t", "-a", "-abc", "bar" }, null, null, out sError));
			Debug.Assert(cl.FIsSwitchSet("t"));
			Debug.Assert(cl.FIsSwitchSet("a"));
			Debug.Assert(!cl.FIsSwitchSet("b"));
			Debug.Assert(cl.FIsSwitchSet("abc"));

			Debug.Assert(String.Compare("bar", cl.ParamFromSwitch("abc")) == 0);

			cl = new CmdLine(cfg);
			Debug.Assert(!cl.FParse(new String[] { }, null, null, out sError));

			cl = new CmdLine(cfg);

			Debug.Assert(!cl.FParse(new string[] { "-t", "-a", "-abc" }, null, null, out sError));
		}

		class UnitTestCmdLineDispatch : ICmdLineDispatch
		{
			public bool m_fTSet;
			public bool m_fASet;
			public bool m_fBSet;
			public bool m_fABCSet;

			public string m_sBParam;
			public string m_sABCParam;

			public bool FDispatchCmdLineSwitch(CmdLineSwitch cls, string sParam, object oClient, out string sError)
			{
                sError = null;
				if (cls.Switch.Length == 1)
					{
					switch (cls.Switch[0])
						{
						case 't':
							m_fTSet = true;
							break;
						case 'a':
							m_fASet = true;
							break;
						case 'b':
							m_fBSet = true;
							m_sBParam = sParam;
							break;
						default:
							sError = String.Format("unknown arg '{0}'", cls.Switch);
							return false;
						}
					}
				else
					{
					if (String.Compare(cls.Switch, "abc") == 0)
						{
						m_fABCSet = true;
						m_sABCParam = sParam;
						}
					else
						{
						sError = String.Format("unknown arg '{0}'", cls.Switch);
						return false;
						}
					}
                return true;
			}
		}

		static void UnitTestDispatch()
		{
			CmdLineConfig cfg = new CmdLineConfig(new CmdLineSwitch[] 
				{
					new CmdLineSwitch("t", true, true, "required toggle switch t", null, null),
					new CmdLineSwitch("a", true, false, "optional toggle switch a", null, null),
					new CmdLineSwitch("b", false, false, "optional switch b", null, null),
					new CmdLineSwitch("abc", false, false, "optional multichar switch abc", null, null)
				} );

			CmdLine cl = new CmdLine(cfg);
			UnitTestCmdLineDispatch cld = new UnitTestCmdLineDispatch();
			string sError;

			Debug.Assert(cl.FParse(new string[] { "-t" }, null, null, out sError));
			cl = new CmdLine(cfg);
			cld = new UnitTestCmdLineDispatch();

			Debug.Assert(cl.FParse(new string[] { "-t", "-a" }, cld, null, out sError));
			Debug.Assert(cld.m_fTSet);
			Debug.Assert(cld.m_fASet);
			Debug.Assert(!cld.m_fBSet);
			Debug.Assert(!cld.m_fABCSet);

			cl = new CmdLine(cfg);
			cld = new UnitTestCmdLineDispatch();

			Debug.Assert(cl.FParse(new string[] { "-t", "-a", "-b", "bar" }, cld, null, out sError));
			Debug.Assert(cld.m_fTSet);
			Debug.Assert(cld.m_fASet);
			Debug.Assert(cld.m_fBSet);
			Debug.Assert(!cld.m_fABCSet);

			Debug.Assert(String.Compare("bar", cld.m_sBParam) == 0);

			cl = new CmdLine(cfg);
			cld = new UnitTestCmdLineDispatch();

			Debug.Assert(cl.FParse(new string[] { "-t", "-a", "-abc", "bar" }, cld, null, out sError));
			Debug.Assert(cld.m_fTSet);
			Debug.Assert(cld.m_fASet);
			Debug.Assert(!cld.m_fBSet);
			Debug.Assert(cld.m_fABCSet);

			Debug.Assert(String.Compare("bar", cld.m_sABCParam) == 0);

			cl = new CmdLine(cfg);
			cld = new UnitTestCmdLineDispatch();

			Debug.Assert(!cl.FParse(new String[] { }, cld, null, out sError));

			cl = new CmdLine(cfg);
			cld = new UnitTestCmdLineDispatch();

			Debug.Assert(!cl.FParse(new string[] { "-t", "-a", "-abc" }, null, null, out sError));
		}

		public static void UnitTest()
		{
			UnitTestNoDispatch();
			UnitTestDispatch();
		}

	}
}

