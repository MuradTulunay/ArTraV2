#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Volumn Indicators
	[System.Serializable]
	public class AMOUNT:FormulaBase
	{
		public double N1=0;
		public double N2=0;
		public AMOUNT():base()
		{
			AddParam("N1","5","1","100","1","",FormulaParamType.Double);
			AddParam("N2","20","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=VOLUME;NONAME0.SetAttrs("VOLSTICK");
			FormulaData MA1=MA(VOLUME,N1); MA1.Name="MA1";
			FormulaData MA2=MA(VOLUME,N2); MA2.Name="MA2";
			return new FormulaPackage(new FormulaData[]{NONAME0,MA1,MA2},"");
		}
	
		public override string LongName
		{
			get{return "AMOUNT";}
		}
	
		public override string Description
		{
			get{return "AMOUNT";}
		}
		public override string OutputFields
		{
			get{return "VOLUME,MA1,MA2";}
		}
	} //class AMOUNT

	[System.Serializable]
	public class VOLMA:FormulaBase
	{
		public double M1=0;
		public VOLMA():base()
		{
			AddParam("M1","60","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData VV=V; VV.Name="VV";VV.SetAttrs("VOLSTICK,HIGH0.2,Alpha100");
			
			SETNAME(VV,"");
			FormulaData MA1=MA(VV,M1); MA1.Name="MA1";
			SETNAME(MA1,"MA");
			
			return new FormulaPackage(new FormulaData[]{VV,MA1},"");
		}
	
		public override string LongName
		{
			get{return "Volumn";}
		}
	
		public override string Description
		{
			get{return "Volumn and moving average";}
		}
		public override string OutputFields
		{
			get{return "VV,MA1";}
		}
	} //class VOLMA

	[System.Serializable]
	public class VOSC:FormulaBase
	{
		public double SHORT=0;
		public double LONG=0;
		public VOSC():base()
		{
			AddParam("SHORT","12","2","50","1","",FormulaParamType.Double);
			AddParam("LONG","26","15","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=(MA(VOL,SHORT)-MA(VOL,LONG))/MA(VOL,SHORT)*100;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Volume Oscillator";}
		}
	
		public override string Description
		{
			get{return "The Volume Oscillator (VO) identifies trends in volume using a two moving average system.\n\nThe Volume Oscillator measures the difference between a faster and slower moving average (MA).\n\nIf the fast MA is above the slow MA the oscillator will be positive.\nIf the fast MA is below the slow MA then the oscillator will be negative.\nThe Volume Oscillator will be zero when the two MA's cross.";}
		}
		public override string OutputFields
		{
			get{return "(MA(VOL,SHORT)-MA(VOL,LONG))/MA(VOL,SHORT)*100";}
		}
	} //class VOSC

	[System.Serializable]
	public class VSTD:FormulaBase
	{
		public double N=0;
		public VSTD():base()
		{
			AddParam("N","10","1","1000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=STD(VOL,N);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Volumn STD";}
		}
	
		public override string Description
		{
			get{return "Volumn STD";}
		}
		public override string OutputFields
		{
			get{return "STD(VOL,N)";}
		}
	} //class VSTD

	[System.Serializable]
	public class PVO:FormulaBase
	{
		public double N1=0;
		public double N2=0;
		public double N3=0;
		public PVO():base()
		{
			AddParam("N1","12","1","100","1","",FormulaParamType.Double);
			AddParam("N2","26","1","100","1","",FormulaParamType.Double);
			AddParam("N3","9","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData E1=EMA(V,N1); E1.Name="E1";
			FormulaData PVO= (E1-EMA(V,N2))/E1*100; PVO.Name="PVO";PVO.SetAttrs("Width1.6,HighQuality");
			FormulaData M= EMA(PVO,N3); M.Name="M";
			return new FormulaPackage(new FormulaData[]{PVO,M},"");
		}
	
		public override string LongName
		{
			get{return "Percentage Volume Oscillator";}
		}
	
		public override string Description
		{
			get{return "The Percentage Volume Oscillator (PVO) is the percentage difference between two moving averages of volume.";}
		}
		public override string OutputFields
		{
			get{return "PVO,M";}
		}
	} //class PVO

	#endregion

} // namespace FML
