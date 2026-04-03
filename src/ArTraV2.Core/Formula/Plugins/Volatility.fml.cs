#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Volatility Indicators
	[System.Serializable]
	public class PSY:FormulaBase
	{
		public double N=0;
		public PSY():base()
		{
			AddParam("N","12","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=COUNT(CLOSE>REF(CLOSE,1),N)/N*100;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Psychogical line";}
		}
	
		public override string Description
		{
			get{return "people resist paying more for a stock than others unless the stock continues to move up.  People resist selling a stock for less than the price others have been getting for it unless the price continues to decline.  People who purchase the stock at the top of a trading range have a strong inclination to wait until the price comes back before they get out.\n";}
		}
		public override string OutputFields
		{
			get{return "COUNT(CLOSE>REF(CLOSE,1),N)/N*100";}
		}
	} //class PSY

	[System.Serializable]
	public class VR:FormulaBase
	{
		public double N=0;
		public VR():base()
		{
			AddParam("N","26","5","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData LC=REF(CLOSE,1); LC.Name="LC";
			FormulaData NONAME0=SUM(IF(CLOSE>LC,VOL,0),N)/
SUM(IF(CLOSE<=LC,VOL,0),N)*100;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Volatility Ratio";}
		}
	
		public override string Description
		{
			get{return "This ratio is derived from the Volatility Ratio introduced by Jack Schwager in Technical Analysis to identify wide-ranging days.\n\nDesigned to highlight breakouts from a trading range, this VR compared to true range for the indicator period.";}
		}
		public override string OutputFields
		{
			get{return "SUM(IF(CLOSE>LC,VOL,0),N)/\nSUM(IF(CLOSE<=LC,VOL,0),N)*100";}
		}
	} //class VR

	[System.Serializable]
	public class ATR:FormulaBase
	{
		public double N=0;
		public ATR():base()
		{
			AddParam("N","10","1","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData ATR=ATRI(N); ATR.Name="ATR";
			return new FormulaPackage(new FormulaData[]{ATR},"");
		}
	
		public override string LongName
		{
			get{return "Average True Range";}
		}
	
		public override string Description
		{
			get{return "The Average True Range indicator was created by J. Welles Wilder. Its primary use is for determining the volatility of the security. The idea is to replace the high - low interval for the given period, as the high-low does not take into consideration gaps and limit moves.";}
		}
		public override string OutputFields
		{
			get{return "ATR";}
		}
	} //class ATR

	[System.Serializable]
	public class VOLATI:FormulaBase
	{
		public double N=0;
		public VOLATI():base()
		{
			AddParam("N","10","2","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData EC= EMA(HIGH-LOW,N); EC.Name="EC";
			FormulaData NONAME0=(EC-REF(EC,N))/REF(EC,N)*100;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Chaikin's Volatility";}
		}
	
		public override string Description
		{
			get{return "Marc Chaikin measures volatility as the trading range between high and low for each period. This does not take trading gaps into account as Average True Range does.";}
		}
		public override string OutputFields
		{
			get{return "(EC-REF(EC,N))/REF(EC,N)*100";}
		}
	} //class VOLATI

	[System.Serializable]
	public class Aroon:FormulaBase
	{
		public double N=0;
		public Aroon():base()
		{
			AddParam("N","25","0","1000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData UP=(N-HHVBARS(C,N))/N*100; UP.Name="Up";
			FormulaData DOWN=(N-LLVBARS(C,N))/N*100; DOWN.Name="Down";
			return new FormulaPackage(new FormulaData[]{UP,DOWN},"");
		}
	
		public override string LongName
		{
			get{return "Aroon";}
		}
	
		public override string Description
		{
			get{return "The Aroon indicator system consists of two lines, 'Aroon(up)' and 'Aroon(down)'. It takes a single parameter which is the number of time periods to use in the calculation. Aroon(up) is the amount of time (on a percentage basis) that has elapsed between the start of the time period and the point at which the highest price during that time period occurred. If the stock closes at a new high for the given period, Aroon(up) will be +100. For each subsequent period that passes without another new high, Aroon(up) moves down by an amount equal to (1 / # of periods) x 100.";}
		}
		public override string OutputFields
		{
			get{return "Up,Down";}
		}
	} //class Aroon

	#endregion

} // namespace FML
