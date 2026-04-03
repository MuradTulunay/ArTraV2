#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Oscillator Indicators
	[System.Serializable]
	public class AD:FormulaBase
	{
		public double N=0;
		public AD():base()
		{
			AddParam("N","20","1","1000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData AD=SUM(((CLOSE-LOW)-(HIGH-CLOSE))/(HIGH-LOW)*VOL,0); AD.Name="AD";
			FormulaData M=MA(AD,N); M.Name="M";
			return new FormulaPackage(new FormulaData[]{AD,M},"");
		}
	
		public override string LongName
		{
			get{return "Accumulation/Distribution";}
		}
	
		public override string Description
		{
			get{return "The Accumulation/Distribution is a momentum indicator that associates changes in price and volume. The indicator is based on the premise that the more volume that accompanies a price move, the more significant the price move.\n\n";}
		}
		public override string OutputFields
		{
			get{return "AD,M";}
		}
	} //class AD

	[System.Serializable]
	public class MI:FormulaBase
	{
		public double N=0;
		public MI():base()
		{
			AddParam("N","12","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData A=C-REF(C,N); A.Name="A";
			FormulaData MI=SMA(A,N,1); MI.Name="MI";
			return new FormulaPackage(new FormulaData[]{A,MI},"");
		}
	
		public override string LongName
		{
			get{return "MI";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "A,MI";}
		}
	} //class MI

	[System.Serializable]
	public class MICD:FormulaBase
	{
		public double N=0;
		public double N1=0;
		public double N2=0;
		public MICD():base()
		{
			AddParam("N","3","1","100","1","",FormulaParamType.Double);
			AddParam("N1","10","1","100","1","",FormulaParamType.Double);
			AddParam("N2","20","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData MI=C-REF(C,1); MI.Name="MI";
			FormulaData AMI=SMA(MI,N,1); AMI.Name="AMI";
			FormulaData DIF=MA(REF(AMI,1),N1)-MA(REF(AMI,1),N2); DIF.Name="DIF";
			FormulaData MICD=SMA(DIF,10,1); MICD.Name="MICD";
			return new FormulaPackage(new FormulaData[]{DIF,MICD},"");
		}
	
		public override string LongName
		{
			get{return "MICD";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "DIF,MICD";}
		}
	} //class MICD

	[System.Serializable]
	public class RC:FormulaBase
	{
		public double N=0;
		public RC():base()
		{
			AddParam("N","50","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData RC=C/REF(C,N); RC.Name="RC";
			FormulaData ARC=SMA(REF(RC,1),N,1); ARC.Name="ARC";
			return new FormulaPackage(new FormulaData[]{ARC},"");
		}
	
		public override string LongName
		{
			get{return "Rate of Change";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "ARC";}
		}
	} //class RC

	[System.Serializable]
	public class RCCD:FormulaBase
	{
		public double N=0;
		public double N1=0;
		public double N2=0;
		public RCCD():base()
		{
			AddParam("N","59","1","100","1","",FormulaParamType.Double);
			AddParam("N1","21","1","100","1","",FormulaParamType.Double);
			AddParam("N2","28","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData RC=C/REF(C,N); RC.Name="RC";
			FormulaData ARC=SMA(REF(RC,1),N,1); ARC.Name="ARC";
			FormulaData DIF=MA(REF(ARC,1),N1)-MA(REF(ARC,1),N2); DIF.Name="DIF";
			FormulaData RCCD=SMA(DIF,N,1); RCCD.Name="RCCD";
			return new FormulaPackage(new FormulaData[]{DIF,RCCD},"");
		}
	
		public override string LongName
		{
			get{return "Rate of Change Convergence Divergence";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "DIF,RCCD";}
		}
	} //class RCCD

	[System.Serializable]
	public class SRMI:FormulaBase
	{
		public double N=0;
		public SRMI():base()
		{
			AddParam("N","9","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=IF(C<REF(C,N),(C-REF(C,N))/REF(C,N),IF(C==REF(C,N),0,(C-REF(C,N))/C));
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "SRMI";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "IF(C<REF(C,N),(C-REF(C,N))/REF(C,N),IF(C==REF(C,N),0,(C-REF(C,N))/C))";}
		}
	} //class SRMI

	[System.Serializable]
	public class CMF:FormulaBase
	{
		public double N=0;
		public CMF():base()
		{
			AddParam("N","20","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData D=(HIGH-LOW); D.Name="D";
			FormulaData AD=IF(D==0,0,((CLOSE-LOW)-(HIGH-CLOSE))/D*VOL); AD.Name="AD";
			FormulaData MV=MA(VOL,N); MV.Name="MV";
			FormulaData CMF=IF(MV==0,0,MA(AD,N)/MV); CMF.Name="CMF";CMF.SetAttrs("COLORSTICK");
			return new FormulaPackage(new FormulaData[]{CMF},"");
		}
	
		public override string LongName
		{
			get{return "Chaikin Money Flow";}
		}
	
		public override string Description
		{
			get{return "Developed by Marc Chaikin, the Chaikin Money Flow oscillator is calculated from the daily readings of the Accumulation/Distribution Line. The basic premise behind the Accumulation Distribution Line is that the degree of buying or selling pressure can be determined by the location of the close relative to the high and low for the corresponding period (Closing Location Value). There is buying pressure when a stock closes in the upper half of a period's range and there is selling pressure when a stock closes in the lower half of the period's trading range. The Closing Location Value multiplied by volume forms the Accumulation/Distribution Value for each period.";}
		}
		public override string OutputFields
		{
			get{return "CMF";}
		}
	} //class CMF

	[System.Serializable]
	public class ULT:FormulaBase
	{
		public double N1=0;
		public double N2=0;
		public double N3=0;
		public ULT():base()
		{
			AddParam("N1","7","1","100","1","",FormulaParamType.Double);
			AddParam("N2","14","1","100","1","",FormulaParamType.Double);
			AddParam("N3","28","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData LC=REF(C,1); LC.Name="LC";
			FormulaData TL=MIN(L,LC); TL.Name="TL";
			FormulaData BP=C-TL; BP.Name="BP";
			FormulaData TR= MAX(H-L,ABS(LC-H),ABS(LC-L)); TR.Name="TR";
			FormulaData BPSUM1= MA(BP,N1); BPSUM1.Name="BPSum1";
			FormulaData BPSUM2= MA(BP,N2); BPSUM2.Name="BPSum2";
			FormulaData BPSUM3= MA(BP,N3); BPSUM3.Name="BPSum3";
			FormulaData TRSUM1= MA(TR,N1); TRSUM1.Name="TRSum1";
			FormulaData TRSUM2= MA(TR,N2); TRSUM2.Name="TRSum2";
			FormulaData TRSUM3= MA(TR,N3); TRSUM3.Name="TRSum3";
			FormulaData RAWUO=4*(BPSUM1/TRSUM1)+2*(BPSUM2/TRSUM2)+(BPSUM3/TRSUM3); RAWUO.Name="RawUO";
			FormulaData NONAME0=(RAWUO/(4+2+1))*100;NONAME0.SetAttrs("Width1.6,HighQuality");
			
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Ultimate Oscillator";}
		}
	
		public override string Description
		{
			get{return "Developed by Larry Williams and first described in a 1985 article for Technical Analysis of Stocks and Commodities magazine, the \"Ultimate\" Oscillator combines a stock's price action during three different time frames into one bounded oscillator. Values range from 0 to 100 with 50 as the center line. Oversold territory exists below 30 and overbought territory extends from 70 to 100.\n\n";}
		}
		public override string OutputFields
		{
			get{return "(RawUO/(4+2+1))*100";}
		}
	} //class ULT

	[System.Serializable]
	public class AroonOsc:FormulaBase
	{
		public double N=0;
		public AroonOsc():base()
		{
			AddParam("N","25","0","1000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=0;
			FormulaData NONAME1=FML(DP,"Aroon(N)[UP]")-FML(DP,"Aroon(N)[DOWN]");
			return new FormulaPackage(new FormulaData[]{NONAME0,NONAME1},"");
		}
	
		public override string LongName
		{
			get{return "Aroon Oscillator";}
		}
	
		public override string Description
		{
			get{return "The Aroon Oscillator was constructed by subtracting Aroon(down) from Aroon(up). Since Aroon(up) and Aroon(down) oscillate between 0 and +100, the Aroon Oscillator oscillate between -100 and +100 with zero as the center crossover line.";}
		}
		public override string OutputFields
		{
			get{return "0,FML(DP,\"Aroon(N)[UP]\")-FML(DP,\"Aroon(N)[DOWN]\")";}
		}
	} //class AroonOsc

	#endregion

} // namespace FML
