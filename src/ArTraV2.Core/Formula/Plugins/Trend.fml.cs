#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Trend Indicators
	[System.Serializable]
	public class BBI:FormulaBase
	{
		public double N1=0;
		public double N2=0;
		public double N3=0;
		public double N4=0;
		public BBI():base()
		{
			AddParam("N1","3","1","100","1","",FormulaParamType.Double);
			AddParam("N2","6","1","100","1","",FormulaParamType.Double);
			AddParam("N3","12","1","100","1","",FormulaParamType.Double);
			AddParam("N4","24","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=(MA(C,N1)+MA(C,N2)+MA(C,N3)+MA(C,N4))/4;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Bullish-Bearish Indicator";}
		}
	
		public override string Description
		{
			get{return "algorithm\n\nBBI = a weighted sum of the other indicators that have a bullish trend.";}
		}
		public override string OutputFields
		{
			get{return "(MA(C,N1)+MA(C,N2)+MA(C,N3)+MA(C,N4))/4";}
		}
	} //class BBI

	[System.Serializable]
	public class CYS:FormulaBase
	{
		public double P1=0;
		public double P2=0;
		public CYS():base()
		{
			AddParam("P1","4","1","15","1","",FormulaParamType.Double);
			AddParam("P2","5","1","15","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData VAR1=IF(YEAR>=2010 & MONTH>=2,0,1); VAR1.Name="Var1";
			FormulaData VAR2=VOL*C; VAR2.Name="Var2";
			FormulaData VAR3=EMA(VAR2,13)/EMA(VOL,13); VAR3.Name="Var3";
			FormulaData CYS= (EMA(CLOSE,P1)-VAR3)/VAR3*100*VAR1; CYS.Name="CYS";
			FormulaData ML= EMA(CYS,P2)*VAR1; ML.Name="ML";
			FormulaData LO= 0; LO.Name="LO";LO.SetAttrs(" POINTDOT");
			return new FormulaPackage(new FormulaData[]{CYS,ML,LO},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "This indicator shows the percentage of buyer which earn money.";}
		}
		public override string OutputFields
		{
			get{return "CYS,ML,LO";}
		}
	} //class CYS

	[System.Serializable]
	public class DDI:FormulaBase
	{
		public double N=0;
		public double N1=0;
		public double M=0;
		public double M1=0;
		public DDI():base()
		{
			AddParam("N","13","1","100","1","",FormulaParamType.Double);
			AddParam("N1","30","1","100","1","",FormulaParamType.Double);
			AddParam("M","10","1","100","1","",FormulaParamType.Double);
			AddParam("M1","5","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TR=MAX(ABS(H-REF(H,1)),ABS(L-REF(L,1))); TR.Name="TR";
			FormulaData DMZ=IF((H+L)<=(REF(H,1)+REF(L,1)),0,MAX(ABS(H-REF(H,1)),ABS(L-REF(L,1)))); DMZ.Name="DMZ";
			FormulaData DMF=IF((H+L)>=(REF(H,1)+REF(L,1)),0,MAX(ABS(H-REF(H,1)),ABS(L-REF(L,1)))); DMF.Name="DMF";
			FormulaData DIZ=SUM(DMZ,N)/(SUM(DMZ,N)+SUM(DMF,N)); DIZ.Name="DIZ";
			FormulaData DIF=SUM(DMF,N)/(SUM(DMF,N)+SUM(DMZ,N)); DIF.Name="DIF";
			FormulaData DDI=DIZ-DIF; DDI.Name="DDI";DDI.SetAttrs("COLORSTICK");
			FormulaData ADDI=SMA(DDI,N1,M); ADDI.Name="ADDI";
			FormulaData AD=MA(ADDI,M1); AD.Name="AD";
			
			return new FormulaPackage(new FormulaData[]{DDI,ADDI,AD},"");
		}
	
		public override string LongName
		{
			get{return "Directional Divergence Index";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "DDI,ADDI,AD";}
		}
	} //class DDI

	[System.Serializable]
	public class DMA:FormulaBase
	{
		public double SHORT=0;
		public double LONG=0;
		public double M=0;
		public DMA():base()
		{
			AddParam("SHORT","10","2","300","1","",FormulaParamType.Double);
			AddParam("LONG","50","2","300","1","",FormulaParamType.Double);
			AddParam("M","10","1","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData DDD= (MA(CLOSE,SHORT)-MA(CLOSE,LONG)); DDD.Name="DDD";
			FormulaData AMA= MA(DDD,M); AMA.Name="AMA";
			return new FormulaPackage(new FormulaData[]{DDD,AMA},"");
		}
	
		public override string LongName
		{
			get{return "Daily Moving Average";}
		}
	
		public override string Description
		{
			get{return "A moving average calculated based upon daily activity. A number preceding the abbreviation indicates the daily time period used to calculate the average.";}
		}
		public override string OutputFields
		{
			get{return "DDD,AMA";}
		}
	} //class DMA

	[System.Serializable]
	public class DMI:FormulaBase
	{
		public double N=0;
		public double M=0;
		public DMI():base()
		{
			AddParam("N","14","2","100","1","",FormulaParamType.Double);
			AddParam("M","6","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TR= SUM(MAX(MAX(HIGH-LOW,ABS(HIGH-REF(CLOSE,1))),ABS(LOW-REF(CLOSE,1))),N); TR.Name="TR";
			FormulaData HD= HIGH-REF(HIGH,1); HD.Name="HD";
			FormulaData LD= REF(LOW,1)-LOW; LD.Name="LD";
			FormulaData DMP= SUM(IF(HD>0 & HD>LD,HD,0),N); DMP.Name="DMP";
			FormulaData DMM= SUM(IF(LD>0 & LD>HD,LD,0),N); DMM.Name="DMM";
			FormulaData PDI= DMP*100/TR; PDI.Name="PDI";
			FormulaData MDI= DMM*100/TR; MDI.Name="MDI";
			FormulaData ADX= MA(ABS(MDI-PDI)/(MDI+PDI)*100,M); ADX.Name="ADX";
			FormulaData ADXR=(ADX+REF(ADX,M))/2; ADXR.Name="ADXR";
			return new FormulaPackage(new FormulaData[]{PDI,MDI,ADX,ADXR},"");
		}
	
		public override string LongName
		{
			get{return "Directional Movement Index";}
		}
	
		public override string Description
		{
			get{return "Directional movement is a system for providing trading signals to be used for price breaks from a trading range.  The system involves 5 indicators which are the Directional Movement Index (DX), the plus Directional Indicator (+DI), the minus Directional Indicator (-DI), the average Directional Movement (ADX) and the Directional movement rating (ADXR).  The system was developed J. Welles Wilder and is explained thoroughly in his book, New Concepts in Technical Trading Systems .\n\nThe basic Directional Movement Trading system involves plotting the 14day +DI and the 14 day -DI on top of each other.  When the +DI rises above the -DI, it is a bullish signal.  A bearish signal occurs when the +DI falls below the -DI.  To avoid whipsaws, Wilder identifies a trigger point to be the extreme price on the day the lines cross.  If you have received a buy signal, you would wait for the security to rise above the extreme price (the high price on the day the lines crossed).  If you are waiting for a sell signal the extreme point is then defined as the low price on the day's the line cross.";}
		}
		public override string OutputFields
		{
			get{return "PDI,MDI,ADX,ADXR";}
		}
	} //class DMI

	[System.Serializable]
	public class EMA4:FormulaBase
	{
		public double P1=0;
		public double P2=0;
		public double P3=0;
		public double P4=0;
		public EMA4():base()
		{
			AddParam("P1","5","1","300","1","",FormulaParamType.Double);
			AddParam("P2","10","1","300","1","",FormulaParamType.Double);
			AddParam("P3","20","1","300","1","",FormulaParamType.Double);
			AddParam("P4","60","1","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData MA1=EMA(CLOSE,P1); MA1.Name="MA1";
			FormulaData MA2=EMA(CLOSE,P2); MA2.Name="MA2";
			FormulaData MA3=EMA(CLOSE,P3); MA3.Name="MA3";
			FormulaData MA4=EMA(CLOSE,P4); MA4.Name="MA4";
			return new FormulaPackage(new FormulaData[]{MA1,MA2,MA3,MA4},"");
		}
	
		public override string LongName
		{
			get{return "Exponential Moving Averages";}
		}
	
		public override string Description
		{
			get{return "An exponential moving average (EMA) is calculated by adding a percentage of yesterday's moving average to a percentage of today's closing value.  In this way an investor can put more emphasis on more recent data and less weight on past data in the calculation of the moving average.";}
		}
		public override string OutputFields
		{
			get{return "MA1,MA2,MA3,MA4";}
		}
	} //class EMA4

	[System.Serializable]
	public class MA4:FormulaBase
	{
		public double P1=0;
		public double P2=0;
		public double P3=0;
		public double P4=0;
		public MA4():base()
		{
			AddParam("P1","5","0","300","1","",FormulaParamType.Double);
			AddParam("P2","10","0","300","1","",FormulaParamType.Double);
			AddParam("P3","20","0","300","1","",FormulaParamType.Double);
			AddParam("P4","30","0","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData MA1=MA(CLOSE,P1); MA1.Name="MA1";
			FormulaData MA2=MA(CLOSE,P2); MA2.Name="MA2";
			FormulaData MA3=MA(CLOSE,P3); MA3.Name="MA3";
			FormulaData MA4=MA(CLOSE,P4); MA4.Name="MA4";
			return new FormulaPackage(new FormulaData[]{MA1,MA2,MA3,MA4},"");
		}
	
		public override string LongName
		{
			get{return "Moving Averages";}
		}
	
		public override string Description
		{
			get{return "Moving averages are used to help identify the trend of prices.  By creating an average of prices, that \"moves\" with the addition of new data, the price action on the security being analyzed is \"smoothed\".  In other words, by calculating the average value of a underlying security or indicator, day to day fluctuations are reduced in importance and what remains is a stronger indication of the trend of prices over the period being analyzed.";}
		}
		public override string OutputFields
		{
			get{return "MA1,MA2,MA3,MA4";}
		}
	} //class MA4

	[System.Serializable]
	public class MACD:FormulaBase
	{
		public double LONG=0;
		public double SHORT=0;
		public double M=0;
		public MACD():base()
		{
			AddParam("LONG","26","20","100","2","",FormulaParamType.Double);
			AddParam("SHORT","12","5","40","2","",FormulaParamType.Double);
			AddParam("M","9","2","60","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData MACD= SETZERO(EMA(CLOSE,SHORT) - EMA(CLOSE,LONG), MAX(SHORT,LONG)-1); MACD.Name="MACD";
			FormulaData TRIGGER= EMA(MACD,M); TRIGGER.Name="TRIGGER";
			FormulaData NONAME0=TRIGGER;
			FormulaData NONAME1=MACD;
			return new FormulaPackage(new FormulaData[]{NONAME0,NONAME1},"");
		}
	
		public override string LongName
		{
			get{return "Moving Average Convergence Divergence";}
		}
	
		public override string Description
		{
			get{return "The Moving Average Convergence/Divergence indicator (MACD) is calculated by subtracting the value of a 26-period exponential moving average from a 12-period exponential moving average (EMA). A 9-period dotted exponential moving average (the \"signal line\") of the difference between the 26 and 12 period EMA is used as the signal line.\n\nThe basic MACD trading rule is to sell when the MACD falls below its 9 day signal line and to buy when the MACD rises above the 9 day signal line.  Traders sometimes vary the calculation period of the signal line and may use different moving average lengths in calculating the MACD dependent on the security and trading strategy.";}
		}
		public override string OutputFields
		{
			get{return "TRIGGER,MACD";}
		}
	} //class MACD

	[System.Serializable]
	public class MTM:FormulaBase
	{
		public double N=0;
		public double N1=0;
		public MTM():base()
		{
			AddParam("N","6","1","100","1","",FormulaParamType.Double);
			AddParam("N1","6","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData MTM= CLOSE-REF(CLOSE,N); MTM.Name="MTM";
			FormulaData MTMMA= MA(MTM,N1); MTMMA.Name="MTMMA";
			return new FormulaPackage(new FormulaData[]{MTM,MTMMA},"");
		}
	
		public override string LongName
		{
			get{return "Momentum";}
		}
	
		public override string Description
		{
			get{return "The Momentum is simply the difference between the current point (price or something else) and the point N periods ago.\n\nUsage:\n\nThe rising line signals that the uptrend is getting stronger, the horizontal line at zero level means there is no trend, and falling line means the downtrend is getting stronger.\n\nThe momentum can be used for identifying trends, overbought/oversold conditions and divergences.\n";}
		}
		public override string OutputFields
		{
			get{return "MTM,MTMMA";}
		}
	} //class MTM

	[System.Serializable]
	public class PPO:FormulaBase
	{
		public double LONG=0;
		public double SHORT=0;
		public double N=0;
		public PPO():base()
		{
			AddParam("LONG","26","5","100","1","",FormulaParamType.Double);
			AddParam("SHORT","12","2","40","1","",FormulaParamType.Double);
			AddParam("N","9","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData PPO=(EMA(CLOSE,SHORT)-EMA(CLOSE,LONG))/EMA(CLOSE,LONG)*100; PPO.Name="PPO";
			FormulaData NONAME0=EMA(PPO,N);
			return new FormulaPackage(new FormulaData[]{PPO,NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Price Oscillator";}
		}
	
		public override string Description
		{
			get{return "An indicator which attempts to assess the momentum of price activity by the use of two or more moving averages, for a predefined time frame period.";}
		}
		public override string OutputFields
		{
			get{return "PPO,EMA(PPO,N)";}
		}
	} //class PPO

	[System.Serializable]
	public class SAR:FormulaBase
	{
		public double N=0;
		public double STEP=0;
		public double MAXP=0;
		public SAR():base()
		{
			AddParam("N","10","1","100","1","Days",FormulaParamType.Double);
			AddParam("STEP","2","1","100","1","",FormulaParamType.Double);
			AddParam("MAXP","20","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=SARTURN(HIGH,LOW,CLOSE,STEP,STEP,MAXP);NONAME0.SetAttrs("CIRCLEDOT");
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Stop and Reverse";}
		}
	
		public override string Description
		{
			get{return "Parabolic Stop and Release was introduced by J Welles Wilder.  The Parabolic SAR is a trend following indicator that is designed to create a trailing stop.  This is a point that follows a prevailing trend, giving a possible value for a stop loss order that is far enough away from the original trend to avoid being stopped out with a small consolidation and retraction moves.  The trailing stop moves with the trend, accelerating closer to price action as time passes giving the path of the indicator a parabolic look.  When price penetrates the SAR a new calculation is started taking the other side of the market with an initial setting that again allows a certain amount of initial volatility if the trend is slow to get underway.";}
		}
		public override string OutputFields
		{
			get{return "SAR(N,STEP,MAXP)";}
		}
	} //class SAR

	[System.Serializable]
	public class TRIX:FormulaBase
	{
		public double N=0;
		public double M=0;
		public TRIX():base()
		{
			AddParam("N","12","3","100","1","",FormulaParamType.Double);
			AddParam("M","20","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TR= EMA(EMA(EMA(CLOSE,N),N),N); TR.Name="TR";
			FormulaData TRIX= (TR-REF(TR,1))/REF(TR,1)*100; TRIX.Name="TRIX";
			FormulaData TRMA=  MA(TRIX,M); TRMA.Name="TRMA";
			
			return new FormulaPackage(new FormulaData[]{TRIX,TRMA},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "TRIX is a momentum indicator that displays the percent rate-of-change of a triple exponentially smoothed moving average of the security's closing price. It is designed to keep you in trends equal to or shorter than the number of periods you specify.\n\n";}
		}
		public override string OutputFields
		{
			get{return "TRIX,TRMA";}
		}
	} //class TRIX

	[System.Serializable]
	public class VMACD:FormulaBase
	{
		public double SHORT=0;
		public double LONG=0;
		public double M=0;
		public VMACD():base()
		{
			AddParam("SHORT","12","1","50","1","",FormulaParamType.Double);
			AddParam("LONG","26","20","100","1","",FormulaParamType.Double);
			AddParam("M","9","30","50","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData DIFF= EMA(VOL,SHORT) - EMA(VOL,LONG); DIFF.Name="DIFF";
			FormulaData DEA= EMA(DIFF,M); DEA.Name="DEA";
			FormulaData MACD= DIFF-DEA; MACD.Name="MACD";MACD.SetAttrs(" COLORSTICK");
			return new FormulaPackage(new FormulaData[]{DIFF,DEA,MACD},"");
		}
	
		public override string LongName
		{
			get{return "Volumn Moving Average Convergence Divergence";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "DIFF,DEA,MACD";}
		}
	} //class VMACD

	[System.Serializable]
	public class ZIG:FormulaBase
	{
		public double PER=0;
		public ZIG():base()
		{
			AddParam("PER","10",".0001","60","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=ZIG(PER);NONAME0.SetAttrs("Width2");
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Zig Zag";}
		}
	
		public override string Description
		{
			get{return "Zig Zag is trend following indicator that helps define what the trend has been, and can be used as a significance test to help determine when changes in the current price might indicate when the trend of price might be changing.  The zig zag indicators filters out changes in a data item that are less than a specific amount that you define.  Below is a chart of National Semiconductor.  If you bought every time the zig zag moved up and sold every time the zig zag moved down, every trade would be a winner.\n\n";}
		}
		public override string OutputFields
		{
			get{return "ZIG(PER)";}
		}
	} //class ZIG

	[System.Serializable]
	public class ADX:FormulaBase
	{
		public double N=0;
		public ADX():base()
		{
			AddParam("N","14","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TR= SUM(MAX(MAX(HIGH-LOW,ABS(HIGH-REF(CLOSE,1))),ABS(LOW-REF(CLOSE,1))),N); TR.Name="TR";
			FormulaData HD= HIGH-REF(HIGH,1); HD.Name="HD";
			FormulaData LD= REF(LOW,1)-LOW; LD.Name="LD";
			FormulaData DMP= SUM(IF(HD>0 & HD>LD,HD,0),N); DMP.Name="DMP";
			FormulaData DMM= SUM(IF(LD>0 & LD>HD,LD,0),N); DMM.Name="DMM";
			FormulaData PDI= DMP*100/TR; PDI.Name="PDI";
			SETNAME(PDI,"+DI");
			FormulaData MDI= DMM*100/TR; MDI.Name="MDI";
			SETNAME(MDI,"-DI");
			FormulaData ADX= MA(ABS(MDI-PDI)/(MDI+PDI)*100,N); ADX.Name="ADX";ADX.SetAttrs("Width2");
			
			return new FormulaPackage(new FormulaData[]{PDI,MDI,ADX},"");
		}
	
		public override string LongName
		{
			get{return "Average Directional Index";}
		}
	
		public override string Description
		{
			get{return "J. Welles Wilder Jr. developed the Average Directional Index (ADX) in order to evaluate the strength of the current trend, be it up or down. It's important to detemine whether the market is trending or trading (moving sideways), because certain indicators give more useful results depending on the market doing one or the other.\n\n";}
		}
		public override string OutputFields
		{
			get{return "PDI,MDI,ADX";}
		}
	} //class ADX

	[System.Serializable]
	public class ICHIMOKU:FormulaBase
	{
		public double TENKAN_PERIOD=0;
		public double KIJUN_PERIOD=0;
		public double SENKOU_B_PERIOD=0;
		public double DISPLACEMENT=0;
		public double SENSITIVITY=0;
		public ICHIMOKU():base()
		{
			AddParam("TENKAN_PERIOD","9","1","50","1","Tenkan-Sen Periyodu (Dönüşüm Çizgisi)",FormulaParamType.Double);
			AddParam("KIJUN_PERIOD","26","1","100","1","Kijun-Sen Periyodu (Temel Çizgi)",FormulaParamType.Double);
			AddParam("SENKOU_B_PERIOD","52","1","200","1","Senkou Span B Periyodu (Öncü Span B)",FormulaParamType.Double);
			AddParam("DISPLACEMENT","26","1","100","1","Senkou Span ve Chikou Span Kaydırma",FormulaParamType.Double);
			AddParam("SENSITIVITY","2","1","3","1","Sinyal Hassasiyeti (1=Tüm, 2=Orta, 3=Sadece güçlü)",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TENKAN_HIGH=HHV(H,TENKAN_PERIOD); TENKAN_HIGH.Name="TENKAN_HIGH";
			FormulaData TENKAN_LOW=LLV(L,TENKAN_PERIOD); TENKAN_LOW.Name="TENKAN_LOW";
			FormulaData TENKAN_SEN=(TENKAN_HIGH+TENKAN_LOW)/2; TENKAN_SEN.Name="TENKAN_SEN";
			FormulaData KIJUN_HIGH=HHV(H,KIJUN_PERIOD); KIJUN_HIGH.Name="KIJUN_HIGH";
			FormulaData KIJUN_LOW=LLV(L,KIJUN_PERIOD); KIJUN_LOW.Name="KIJUN_LOW";
			FormulaData KIJUN_SEN=(KIJUN_HIGH+KIJUN_LOW)/2; KIJUN_SEN.Name="KIJUN_SEN";
			FormulaData SENKOU_SPAN_A=REF((TENKAN_SEN+KIJUN_SEN)/2,-DISPLACEMENT); SENKOU_SPAN_A.Name="SENKOU_SPAN_A";
			FormulaData SENKOU_B_HIGH=HHV(H,SENKOU_B_PERIOD); SENKOU_B_HIGH.Name="SENKOU_B_HIGH";
			FormulaData SENKOU_B_LOW=LLV(L,SENKOU_B_PERIOD); SENKOU_B_LOW.Name="SENKOU_B_LOW";
			FormulaData SENKOU_SPAN_B=REF((SENKOU_B_HIGH+SENKOU_B_LOW)/2,-DISPLACEMENT); SENKOU_SPAN_B.Name="SENKOU_SPAN_B";
			FormulaData CHIKOU_SPAN=REF(C,DISPLACEMENT); CHIKOU_SPAN.Name="CHIKOU_SPAN";
			FormulaData BASIC_BUY=CROSS(TENKAN_SEN,KIJUN_SEN); BASIC_BUY.Name="BASIC_BUY";
			FormulaData BASIC_SELL=CROSS(KIJUN_SEN,TENKAN_SEN); BASIC_SELL.Name="BASIC_SELL";
			FormulaData BULUT_YUKARI=SENKOU_SPAN_A>SENKOU_SPAN_B; BULUT_YUKARI.Name="BULUT_YUKARI";
			FormulaData BULUT_ASAGI=SENKOU_SPAN_A<SENKOU_SPAN_B; BULUT_ASAGI.Name="BULUT_ASAGI";
			FormulaData FIYAT_BULUT_USTU=C>SENKOU_SPAN_A & C>SENKOU_SPAN_B; FIYAT_BULUT_USTU.Name="FIYAT_BULUT_USTU";
			FormulaData FIYAT_BULUT_ALTI=C<SENKOU_SPAN_A & C<SENKOU_SPAN_B; FIYAT_BULUT_ALTI.Name="FIYAT_BULUT_ALTI";
			FormulaData CHIKOU_YUKARI=CHIKOU_SPAN>REF(C,2*DISPLACEMENT); CHIKOU_YUKARI.Name="CHIKOU_YUKARI";
			FormulaData CHIKOU_ASAGI=CHIKOU_SPAN<REF(C,2*DISPLACEMENT); CHIKOU_ASAGI.Name="CHIKOU_ASAGI";
			FormulaData STRONG_BUY=BASIC_BUY & BULUT_YUKARI & FIYAT_BULUT_USTU & CHIKOU_YUKARI; STRONG_BUY.Name="STRONG_BUY";
			FormulaData STRONG_SELL=BASIC_SELL & BULUT_ASAGI & FIYAT_BULUT_ALTI & CHIKOU_ASAGI; STRONG_SELL.Name="STRONG_SELL";
			FormulaData MEDIUM_BUY=BASIC_BUY & (BULUT_YUKARI | FIYAT_BULUT_USTU | CHIKOU_YUKARI); MEDIUM_BUY.Name="MEDIUM_BUY";
			FormulaData MEDIUM_SELL=BASIC_SELL & (BULUT_ASAGI | FIYAT_BULUT_ALTI | CHIKOU_ASAGI); MEDIUM_SELL.Name="MEDIUM_SELL";
			FormulaData BUY_SIGNAL=IF(SENSITIVITY==3,STRONG_BUY,IF(SENSITIVITY==2,MEDIUM_BUY,BASIC_BUY)); BUY_SIGNAL.Name="BUY_SIGNAL";
			FormulaData SELL_SIGNAL=IF(SENSITIVITY==3,STRONG_SELL,IF(SENSITIVITY==2,MEDIUM_SELL,BASIC_SELL)); SELL_SIGNAL.Name="SELL_SIGNAL";
			FormulaData NONAME0=DRAWICON(SELL_SIGNAL,H,"Sell.Gif");NONAME0.SetAttrs("Top");
			FormulaData NONAME1=DRAWICON(BUY_SIGNAL,L,"Buy.Gif");NONAME1.SetAttrs("Bottom");
			return new FormulaPackage(new FormulaData[]{TENKAN_SEN,KIJUN_SEN,SENKOU_SPAN_A,SENKOU_SPAN_B,CHIKOU_SPAN,NONAME0,NONAME1},"");
		}
	
		public override string LongName
		{
			get{return "Ichimoku Kinko Hyo";}
		}
	
		public override string Description
		{
			get{return "Ichimoku Kinko Hyo (Ichimoku Cloud) - Japon teknik analiz indikatörü. Tenkan-Sen (9 dönem), Kijun-Sen (26 dönem), Senkou Span A ve B çizgileri ile Chikou Span'dan oluşur. Trend yönü, destek/direnç seviyeleri ve momentum analizi için kullanılır. Alım satım sinyalleri içerir.";}
		}
		public override string OutputFields
		{
			get{return "TENKAN_SEN,KIJUN_SEN,SENKOU_SPAN_A,SENKOU_SPAN_B,CHIKOU_SPAN,DRAWICON(SELL_SIGNAL,H,\"Sell.Gif\"),DRAWICON(BUY_SIGNAL,L,\"Buy.Gif\")";}
		}
	} //class ICHIMOKU

	[System.Serializable]
	public class ZIGT:FormulaBase
	{
		public double PER=0;
		public double ITERATION=0;
		public double LIM=0;
		public ZIGT():base()
		{
			AddParam("PER","1","0.0001","10","1","Percent",FormulaParamType.Double);
			AddParam("ITERATION","-1","-1","100","1","Trend iteration count",FormulaParamType.Double);
			AddParam("LIM","0","0.0001","10","1","Trend limit",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData ZIGTR=ZIG(CLOSE,PER); ZIGTR.Name="ZIGTr";ZIGTR.SetAttrs("Width2");
			return new FormulaPackage(new FormulaData[]{ZIGTR},"");
		}
	
		public override string LongName
		{
			get{return "Zig-Zag Trender";}
		}
	
		public override string Description
		{
			get{return "Trends optimized ZigZag";}
		}
		public override string OutputFields
		{
			get{return "ZIGTr";}
		}
	} //class ZIGT

	[System.Serializable]
	public class T_ICHIMOKU:FormulaBase
	{
		public double TENKAN_PERIOD=0;
		public double KIJUN_PERIOD=0;
		public double SENKOU_B_PERIOD=0;
		public double DISPLACEMENT=0;
		public double SENSITIVITY=0;
		public double ONMAIN=0;
		public double SL=0;
		public T_ICHIMOKU():base()
		{
			AddParam("TENKAN_PERIOD","9","1","50","1","Tenkan-Sen Periyodu (Dönüşüm Çizgisi)",FormulaParamType.Double);
			AddParam("KIJUN_PERIOD","26","1","100","1","Kijun-Sen Periyodu (Temel Çizgi)",FormulaParamType.Double);
			AddParam("SENKOU_B_PERIOD","52","1","200","1","Senkou Span B Periyodu (Öncü Span B)",FormulaParamType.Double);
			AddParam("DISPLACEMENT","26","1","100","1","Senkou Span ve Chikou Span Kaydırma",FormulaParamType.Double);
			AddParam("SENSITIVITY","2","1","3","1","Sinyal Hassasiyeti (1=Tüm, 2=Orta, 3=Sadece güçlü)",FormulaParamType.Double);
			AddParam("onMain","0","0","1","1","",FormulaParamType.Double);
			AddParam("SL","1","0.001","10",".001","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TENKAN_HIGH=HHV(H,TENKAN_PERIOD); TENKAN_HIGH.Name="TENKAN_HIGH";
			FormulaData TENKAN_LOW=LLV(L,TENKAN_PERIOD); TENKAN_LOW.Name="TENKAN_LOW";
			FormulaData TENKAN_SEN=(TENKAN_HIGH+TENKAN_LOW)/2; TENKAN_SEN.Name="TENKAN_SEN";
			FormulaData KIJUN_HIGH=HHV(H,KIJUN_PERIOD); KIJUN_HIGH.Name="KIJUN_HIGH";
			FormulaData KIJUN_LOW=LLV(L,KIJUN_PERIOD); KIJUN_LOW.Name="KIJUN_LOW";
			FormulaData KIJUN_SEN=(KIJUN_HIGH+KIJUN_LOW)/2; KIJUN_SEN.Name="KIJUN_SEN";
			FormulaData SENKOU_SPAN_A=REF((TENKAN_SEN+KIJUN_SEN)/2,-DISPLACEMENT); SENKOU_SPAN_A.Name="SENKOU_SPAN_A";
			FormulaData SENKOU_B_HIGH=HHV(H,SENKOU_B_PERIOD); SENKOU_B_HIGH.Name="SENKOU_B_HIGH";
			FormulaData SENKOU_B_LOW=LLV(L,SENKOU_B_PERIOD); SENKOU_B_LOW.Name="SENKOU_B_LOW";
			FormulaData SENKOU_SPAN_B=REF((SENKOU_B_HIGH+SENKOU_B_LOW)/2,-DISPLACEMENT); SENKOU_SPAN_B.Name="SENKOU_SPAN_B";
			FormulaData CHIKOU_SPAN=REF(C,DISPLACEMENT); CHIKOU_SPAN.Name="CHIKOU_SPAN";
			FormulaData BASIC_BUY=CROSS(TENKAN_SEN,KIJUN_SEN); BASIC_BUY.Name="BASIC_BUY";
			FormulaData BASIC_SELL=CROSS(KIJUN_SEN,TENKAN_SEN); BASIC_SELL.Name="BASIC_SELL";
			FormulaData BULUT_YUKARI=SENKOU_SPAN_A>SENKOU_SPAN_B; BULUT_YUKARI.Name="BULUT_YUKARI";
			FormulaData BULUT_ASAGI=SENKOU_SPAN_A<SENKOU_SPAN_B; BULUT_ASAGI.Name="BULUT_ASAGI";
			FormulaData FIYAT_BULUT_USTU=C>SENKOU_SPAN_A & C>SENKOU_SPAN_B; FIYAT_BULUT_USTU.Name="FIYAT_BULUT_USTU";
			FormulaData FIYAT_BULUT_ALTI=C<SENKOU_SPAN_A & C<SENKOU_SPAN_B; FIYAT_BULUT_ALTI.Name="FIYAT_BULUT_ALTI";
			FormulaData CHIKOU_YUKARI=CHIKOU_SPAN>REF(C,2*DISPLACEMENT); CHIKOU_YUKARI.Name="CHIKOU_YUKARI";
			FormulaData CHIKOU_ASAGI=CHIKOU_SPAN<REF(C,2*DISPLACEMENT); CHIKOU_ASAGI.Name="CHIKOU_ASAGI";
			FormulaData STRONG_BUY=BASIC_BUY & BULUT_YUKARI & FIYAT_BULUT_USTU & CHIKOU_YUKARI; STRONG_BUY.Name="STRONG_BUY";
			FormulaData STRONG_SELL=BASIC_SELL & BULUT_ASAGI & FIYAT_BULUT_ALTI & CHIKOU_ASAGI; STRONG_SELL.Name="STRONG_SELL";
			FormulaData MEDIUM_BUY=BASIC_BUY & (BULUT_YUKARI | FIYAT_BULUT_USTU | CHIKOU_YUKARI); MEDIUM_BUY.Name="MEDIUM_BUY";
			FormulaData MEDIUM_SELL=BASIC_SELL & (BULUT_ASAGI | FIYAT_BULUT_ALTI | CHIKOU_ASAGI); MEDIUM_SELL.Name="MEDIUM_SELL";
			FormulaData BUY=IF(SENSITIVITY==3,STRONG_BUY,IF(SENSITIVITY==2,MEDIUM_BUY,BASIC_BUY)); BUY.Name="BUY";
			FormulaData SELL=IF(SENSITIVITY==3,STRONG_SELL,IF(SENSITIVITY==2,MEDIUM_SELL,BASIC_SELL)); SELL.Name="SELL";
			FormulaData ALTBUY=EMPTY; ALTBUY.Name="ALTBUY";
			FormulaData ALTSELL=EMPTY; ALTSELL.Name="ALTSELL";
			FormulaData OPENSELL=EMPTY; OPENSELL.Name="OPENSELL";OPENSELL.SetAttrs("ColorDarkOrange");
			FormulaData CLOSEOPENSELL=EMPTY; CLOSEOPENSELL.Name="CLOSEOPENSELL";CLOSEOPENSELL.SetAttrs("ColorDarkViolet");
			FormulaData STOPLOSS=EMPTY; STOPLOSS.Name="STOPLOSS";
			FormulaData BALANCE= 10000.0; BALANCE.Name="BALANCE";
			FormulaData BT= IF(ONMAIN, C, BACKTEST(C,BUY,SELL,OPENSELL,CLOSEOPENSELL,BALANCE[0],0.000342,1,ALTBUY,ALTSELL,SL,STOPLOSS,0.0,FALSE)); BT.Name="BT";
			FormulaData NONAME0=DRAWICON(BUY, IF(ONMAIN, C, BT) ,"Buy.Gif");NONAME0.SetAttrs("Bottom");
			FormulaData NONAME1=DRAWICON(SELL, IF(ONMAIN, C, BT) ,"Sell.Gif");NONAME1.SetAttrs("Top");
			FormulaData NONAME2=DRAWICON(OPENSELL, IF(ONMAIN, C, BT) ,"OpenSell.Gif");NONAME2.SetAttrs("Top");
			FormulaData NONAME3=DRAWICON(CLOSEOPENSELL, IF(ONMAIN, C, BT) ,"CloseOpenSell.Gif");NONAME3.SetAttrs("Bottom");
			FormulaData NONAME4=DRAWICON(IF(ONMAIN, 0, ALTBUY), BT ,"AltBuy.Gif");NONAME4.SetAttrs("Bottom");
			FormulaData NONAME5=DRAWICON(IF(ONMAIN, 0, ALTSELL), BT ,"AltSell.Gif");NONAME5.SetAttrs("Top");
			FormulaData STOPBUYS= STOPLOSS>0; STOPBUYS.Name="STOPBUYS";
			FormulaData STOPSELLS=STOPLOSS<0; STOPSELLS.Name="STOPSELLS";
			FormulaData NONAME6=DRAWICON(IF(ONMAIN, 0, STOPBUYS), BT ,"StopBuy.Gif");NONAME6.SetAttrs("Bottom");
			FormulaData NONAME7=DRAWICON(IF(ONMAIN, 0, STOPSELLS), BT ,"StopSell.Gif");NONAME7.SetAttrs("Top");
			FormulaData MAXPERF= IF(ONMAIN, EMPTY, (BALANCE/LLV(L))*HHV(H)); MAXPERF.Name="MaxPerf";
			FormulaData NONAME8=IF(ONMAIN, EMPTY, BT);
			FormulaData NONAME9=IF(ONMAIN, EMPTY, MAXPERF);NONAME9.SetAttrs(" DarkGreen,Width2");
			return new FormulaPackage(new FormulaData[]{TENKAN_SEN,KIJUN_SEN,SENKOU_SPAN_A,SENKOU_SPAN_B,CHIKOU_SPAN,NONAME0,NONAME1,NONAME2,NONAME3,NONAME4,NONAME5,NONAME6,NONAME7,NONAME8,NONAME9},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "Ichimoku Kinko Hyo (Ichimoku Cloud) - Japon teknik analiz indikatörü. Tenkan-Sen (9 dönem), Kijun-Sen (26 dönem), Senkou Span A ve B çizgileri ile Chikou Span'dan oluşur. Trend yönü, destek/direnç seviyeleri ve momentum analizi için kullanılır. Alım satım sinyalleri içerir.";}
		}
		public override string OutputFields
		{
			get{return "TENKAN_SEN,KIJUN_SEN,SENKOU_SPAN_A,SENKOU_SPAN_B,CHIKOU_SPAN,DRAWICON(BUY, if(OnMain, C, BT) ,\"Buy.Gif\"),DRAWICON(SELL, if(OnMain, C, BT) ,\"Sell.Gif\"),DRAWICON(OPENSELL, if(OnMain, C, BT) ,\"OpenSell.Gif\"),DRAWICON(CLOSEOPENSELL, if(OnMain, C, BT) ,\"CloseOpenSell.Gif\"),DRAWICON(if(OnMain, 0, ALTBUY), BT ,\"AltBuy.Gif\"),DRAWICON(if(OnMain, 0, ALTSELL), BT ,\"AltSell.Gif\"),DRAWICON(if(OnMain, 0, STOPBUYS), BT ,\"StopBuy.Gif\"),DRAWICON(if(OnMain, 0, STOPSELLS), BT ,\"StopSell.Gif\"),if(OnMain, EMPTY, BT),if(OnMain, EMPTY, MaxPerf)";}
		}
	} //class T_ICHIMOKU

	#endregion

} // namespace FML
