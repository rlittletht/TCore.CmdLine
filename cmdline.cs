using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

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
        private bool m_fPositional;

		string m_sParamValue;
		bool m_fParsed;

        public int PositionIndex { get; set; } // this is set when we know what position this arg is actually at
		public string Switch { get { return m_sSwitch; } }
		public bool Toggle { get { return m_fToggle; } }
        public bool Positional {  get {  return m_fPositional; } }
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
            if (string.IsNullOrEmpty(sSwitch))
                m_fPositional = true;

			m_sSwitch = sSwitch;
			m_fToggle = fToggle;
            if (m_fToggle && m_fPositional)
                throw new Exception("cannot have a positional argument that is also a toggle argument");

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

        private List<CmdLineSwitch> m_plPositionalArgs;

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
                if (string.IsNullOrEmpty(cls.Switch))
                {
                    if (m_plPositionalArgs == null)
                        m_plPositionalArgs = new List<CmdLineSwitch>();

                    m_plPositionalArgs.Add(cls);
                }
                else if (cls.Switch.Length == 1)
                {
                    if (m_mpchSwitch == null)
                        m_mpchSwitch = new Dictionary<char, CmdLineSwitch>();

                    m_mpchSwitch.Add(cls.Switch[0], cls);
                }
                else if (cls.Switch.Length > 1)
                {
                    if (m_mpsSwitch == null)
                        m_mpsSwitch = new Dictionary<string, CmdLineSwitch>();

                    m_mpsSwitch.Add(cls.Switch, cls);
                }

                cls.Reset();
            }
            
            // verify positional args make sense
            if (m_plPositionalArgs != null)
            {
                bool fMustBeNotRequired = false;

                foreach (CmdLineSwitch cls in m_plPositionalArgs)
                {
                    if (cls.Toggle)
                        throw new Exception("positional args cannot be toggle args");

                    if (!cls.Positional)
                        throw new Exception("cannot have a non-positional argument without a switch");

                    if (fMustBeNotRequired && cls.Required)
                        throw new Exception("cannot have required positional argument following an optional positional argument");

                    if (!cls.Required)
                        fMustBeNotRequired = true;
                }
            }
        }

        CmdLineSwitch ClsFromArg(string sArg, ref int iPositional)
        {
            if (sArg[0] != '-')
            {
                if (m_plPositionalArgs == null)
                    return null;

                if (iPositional >= m_plPositionalArgs.Count)
                    return null;

                CmdLineSwitch cls = m_plPositionalArgs[iPositional];
                // can only bind to one positional index. set that here.
                cls.PositionIndex = iPositional++;

                return cls;
            }

            return ClsFromSwitch(sArg.Substring(1));
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

            if (m_plPositionalArgs != null)
            {
                foreach (CmdLineSwitch cls in m_plPositionalArgs)
                {
                    if (cls.Required)
                        clo(String.Format("<{0}>\t\t{1}", cls.ArgDescription, cls.Description));
                    else
                    {
                        clo(String.Format("[{0}] <{1}>\t\t{2}", cls.ArgDescription, cls.Description));
                    }
                }
            }

        }

        /* P A R S E */
        /*----------------------------------------------------------------------------
			%%Function: Parse
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
        public bool FParse(string[] rgsArgs, ICmdLineDispatch icld, object oClient, out string sError)
        {
            string sArg;
            int i;
            int iPositionalNext = 0; // what is the next position dependent arg we will have (if we get one)

            sError = null;

            if (rgsArgs == null)
                return true;

            for (i = 0; i < rgsArgs.Length; i++)
            {
                CmdLineSwitch cls = null;
                string sParam = null;

                if (rgsArgs[i][0] != '-' && (m_plPositionalArgs == null || m_plPositionalArgs.Count <= iPositionalNext))
                {
                    sError = String.Format("argument '{0}' missing switch delimeter '-'", rgsArgs[i]);
                    return false;
                }

                sArg = rgsArgs[i];
                cls = ClsFromArg(sArg, ref iPositionalNext);

                if (cls == null)
                {
                    sError = String.Format("argument '{0}' illegal", rgsArgs[i]);
                    return false;
                }

                if (cls.Positional)
                {
                    sParam = rgsArgs[i]; // don't pre-increment i here, there is no switch to skip
                }
                else if (!cls.Toggle)
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

            if (m_plPositionalArgs != null)
            {
                foreach (CmdLineSwitch cls in m_plPositionalArgs)
                {
                    if (cls.Required && !cls.Parsed)
                    {
                        sError = String.Format("required positional parameter not found");
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

        public string GetPositionalArg(int iPositional)
        {
            if (m_plPositionalArgs == null || iPositional >= m_plPositionalArgs.Count)
                return null;

            return m_plPositionalArgs[iPositional].ParamValue;
        }
    }
}
