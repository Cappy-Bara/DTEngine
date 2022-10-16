using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Contracts
{
    public class InputData
    {
        //Tu powinny być chyba tablice\
        public int HorizontalElementsQuantity { get; set; }         //nlh
        public int VerticalElementsQuantity { get; set; }           //nlv
        public float Height { get; set; }                           //h
        public float Width { get; set; }                            //r
        
        
        public float HeatExchangingFactor1 { get; set; } //alfa1
        public float HeatExchangingFactor2 { get; set; } //alfa2
        public float HeatExchangingFactor3 { get; set; } //alfa3
        public float HeatExchangingFactor4 { get; set; } //alfa4


        public float ConductingFactorX { get; set; }     //lambda1
        public float ConductingFactorY { get; set; }     //lambda2


        public float HeatSourcePower { get; set; }  //moc_zrodel

        public float TemperatureBottom { get; set; }    //t_czynnika1;
        public float TemperatureTop { get; set; }    //t_czynnika2;
        public float TemperatureLeft { get; set; }    //t_czynnika3;
        public float TemperatureRight { get; set; }    //t_czynnika4;
    }
}
