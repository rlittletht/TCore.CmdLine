using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;

// USAGE:
//
// Instantiate your command line configuration with CmdLineConfig, then create a CmdLine instance passing
// that configuration to it. Now parse your command line using CmdLine.FParse, passing in a class that
// implements the ICmdLineDispatch interface to allow settings to be set (this can be your core class
// implementing that interface, or it can be a settings object implementing that settings object)

// Example:
//
//  class Foo : TCore.CmdLine.ICmdLineDispatch
//  {
//      string m_fReordToFile = false;
//
//      bool FDispatchCmdLineSwitch(TCore.CmdLine.CmdLineSwitch cls, string sParam, object oClient, out string sError)
//      {
//          ...
//      }
//
//      void Init(string[] args)
//      {
//          CmdLineConfig clcfg = new CmdLineConfig(new CmdLineSwitch[]
//              {
//              new CmdLineSwitch("r", false, false, "record to a file", "filename", null),
//              };
//
//          CmdLine cl = new CmdLine(clcfg);          
//          string sError;
//          if (!cl.FParse(args, this, null, out sError)
//              {
//              throw new Exception(sError);
//              }
//          ...

namespace TCore.CmdLine
{
	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	// I  C M D  L I N E  D I S P A T C H 
	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	public interface ICmdLineDispatch
	{
		bool FDispatchCmdLineSwitch(CmdLineSwitch cls, string sParam, object oClient, out string sError);
	}

	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	// C M D  L I N E  S W I T C H 
	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	public class CmdLineSwitch
	{
		string m_sSwitch;
		bool m_fToggle;
		string m_sDescription;
		string m_sArgDescription;
		bool m_fRequired;
		object m_oClient;

		string m_sParamValue;
		bool m_fParsed;

		public string Switch { get { return m_sSwitch; } }
		public bool Toggle { get { return m_fToggle; } }
		public string Description { get { return m_sDescription; } }
		public string ArgDescription { get { return m_sArgDescription; } }
		public bool Required { get { return m_fRequired; } }
		public object Client { get { return m_oClient; } }
		public string ParamValue { get { return m_sParamValue; } set { m_sParamValue = value; } }
		public bool Parsed { get { return m_fParsed; } set { m_fParsed = value; } }

		/* R E S E T */
		/*----------------------------------------------------------------------------
			%%Function: Reset
			%%Qualified: CmdLineSupport.CmdLineSwitch.Reset
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void Reset()
		{
			m_sParamValue = null;
			m_fParsed = false;
		}

		/* C M D  L I N E  S W I T C H */
		/*----------------------------------------------------------------------------
			%%Function: CmdLineSwitch
			%%Qualified: CmdLineSupport.CmdLineSwitch.CmdLineSwitch
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public CmdLineSwitch(string sSwitch, bool fToggle, bool fRequired, string sDescription, string sArgDescription, object oClient)
		{
			m_sSwitch = sSwitch;
			m_fToggle = fToggle;
			m_sDescription = sDescription;
			m_sArgDescription = sArgDescription;
			m_fRequired = fRequired;
			m_oClient = oClient;
		}
	}

	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	// C M D  L I N E  C O N F I G 
	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	public class CmdLineConfig
	{
		CmdLineSwitch []m_rgcls;

		public CmdLineSwitch[] Switches { get { return m_rgcls; } }

		/* C M D  L I N E  C O N F I G */
		/*----------------------------------------------------------------------------
			%%Function: CmdLineConfig
			%%Qualified: CmdLineSupport.CmdLineConfig.CmdLineConfig
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public CmdLineConfig(CmdLineSwitch[] rgcls)
		{
			m_rgcls = rgcls;
		}
	}

	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	// C M D  L I N E 
	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	public partial class CmdLine
    {
		CmdLineConfig m_cfg;
		Dictionary<char, CmdLineSwitch> m_mpchSwitch;
		Dictionary<string, CmdLineSwitch> m_mpsSwitch;

		/* C M D  L I N E */
		/*----------------------------------------------------------------------------
			%%Function: CmdLine
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public CmdLine(CmdLineConfig cfg)
		{
			m_cfg = cfg;

			foreach (CmdLineSwitch cls in cfg.Switches)
				{
				if (cls.Switch.Length == 1)
					{
					if (m_mpchSwitch == null)
						m_mpchSwitch = new Dictionary<char, CmdLineSwitch>();

					m_mpchSwitch.Add(cls.Switch[0], cls);
					}
				else
					{
					if (m_mpsSwitch == null)
						m_mpsSwitch = new Dictionary<string, CmdLineSwitch>();

					m_mpsSwitch.Add(cls.Switch, cls);
					}
       			cls.Reset();
				}
		}

		/* C L S  F R O M  S W I T C H */
		/*----------------------------------------------------------------------------
			%%Function: ClsFromSwitch
			%%Qualified: CmdLineSupport.CmdLine.ClsFromSwitch
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		CmdLineSwitch ClsFromSwitch(string sSwitch)
		{
			CmdLineSwitch cls = null;

			if (sSwitch.Length == 1)
				{
				if (m_mpchSwitch.ContainsKey(sSwitch[0]))
					cls = m_mpchSwitch[sSwitch[0]];
				}
			else
				{
				if (m_mpsSwitch.ContainsKey(sSwitch))
					cls = m_mpsSwitch[sSwitch];
				}

			return cls;
		}

        public delegate void CmdLineOutput(string s);

        public void Usage(CmdLineOutput clo)
        {
            clo(String.Format("Usage..."));

            if (m_mpchSwitch != null)
                {
                foreach (char ch in m_mpchSwitch.Keys)
                    {
                    CmdLineSwitch cls = m_mpchSwitch[ch];

                    if (cls.Toggle)
                        clo(String.Format("-{0}\t\t{1}", cls.Switch, cls.Description));
                    else
                        {
                        clo(String.Format("-{0} <{1}>\t\t{2}", cls.Switch, cls.ArgDescription, cls.Description));
                        }
                    }
                }

            if (m_mpsSwitch != null)
                {
                foreach (string s in m_mpsSwitch.Keys)
                    {
                    CmdLineSwitch cls = m_mpsSwitch[s];

                    if (cls.Toggle)
                        clo(String.Format("-{0}\t\t{1}", cls.Switch, cls.Description));
                    else
                        {
                        clo(String.Format("-{0} <{1}>\t\t{2}", cls.Switch, cls.ArgDescription, cls.Description));
                        }
                    }
                }
        }

		/* P A R S E */
		/*----------------------------------------------------------------------------
			%%Function: Parse
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public bool FParse(string []rgsArgs, ICmdLineDispatch icld, object oClient, out string sError)
		{
			string sSwitch;
			int i;

            sError = null;

		    if (rgsArgs == null)
		        return true;

			for (i = 0; i < rgsArgs.Length; i++)
				{
				CmdLineSwitch cls = null;
				string sParam = null;

				if (rgsArgs[i][0] != '-')
					{
					sError = String.Format("argument '{0}' missing switch delimeter '-'", rgsArgs[i]);
					return false;
					}
				
				sSwitch = rgsArgs[i].Substring(1);
				cls = ClsFromSwitch(sSwitch);

				if (cls == null)
					{
					sError = String.Format("argument '{0}' illegal", rgsArgs[i]);
					return false;
					}

				if (!cls.Toggle)
					{
					if (++i >= rgsArgs.Length)
						{
						sError = String.Format("expected argument to option '{0}' not found", rgsArgs[i - 1]);
						return false;
						}

					sParam = rgsArgs[i];
					}

				if (icld != null)
					{
					if (!icld.FDispatchCmdLineSwitch(cls, sParam, oClient, out sError))
						return false;
					}

				cls.ParamValue = sParam;
				cls.Parsed = true;
				}

			// lastly, walk through all the params and make sure we have found all the required ones
			if (m_mpchSwitch != null)
				{
				foreach (CmdLineSwitch cls in m_mpchSwitch.Values)
					{
					if (cls.Required && !cls.Parsed)
						{
						sError = String.Format("required parameter '{0}' not found", cls.Switch);
						return false;
						}
					}
				}

			if (m_mpsSwitch != null)
				{
				foreach (CmdLineSwitch cls in m_mpsSwitch.Values)
					{
					if (cls.Required && !cls.Parsed)
						{
						sError = String.Format("required parameter '{0}' not found", cls.Switch);
						return false;
						}
					}
				}

			return true;
		}

		/* F  I S  S W I T C H  S E T */
		/*----------------------------------------------------------------------------
			%%Function: FIsSwitchSet
			%%Qualified: CmdLineSupport.CmdLine.FIsSwitchSet
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public bool FIsSwitchSet(string sSwitch)
		{
			CmdLineSwitch cls = ClsFromSwitch(sSwitch);

			if (cls == null)
				return false;

			return (cls.Parsed || cls.ParamValue != null);
		}

		/* P A R A M  F R O M  S W I T C H */
		/*----------------------------------------------------------------------------
			%%Function: ParamFromSwitch
			%%Qualified: CmdLineSupport.CmdLine.ParamFromSwitch
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public string ParamFromSwitch(string sSwitch)
		{
			CmdLineSwitch cls = ClsFromSwitch(sSwitch);

			if (cls == null)
				return null;

			return cls.ParamValue;
		}

    }
}
