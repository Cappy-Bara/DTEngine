using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Contracts
{
    public class InputData
    {
        public int HorizontalNodesQuantity { get; set; }         //nlh
        public int VerticalNodesQuantity { get; set; }           //nlv
        public decimal Height { get; set; }                           //h
        public decimal Width { get; set; }                            //r
        
        
        public decimal HeatExchangingFactor1 { get; set; } //alfa1
        public decimal HeatExchangingFactor2 { get; set; } //alfa2
        public decimal HeatExchangingFactor3 { get; set; } //alfa3
        public decimal HeatExchangingFactor4 { get; set; } //alfa4


        public decimal ConductingFactorX { get; set; }     //lambda1
        public decimal ConductingFactorY { get; set; }     //lambda2


        public decimal HeatSourcePower { get; set; }  //moc_zrodel
        public decimal HeatStream { get; set; }  //strumien ciepla

        public decimal TemperatureBottom { get; set; }    //t_czynnika1;
        public decimal TemperatureTop { get; set; }    //t_czynnika2;
        public decimal TemperatureLeft { get; set; }    //t_czynnika3;
        public decimal TemperatureRight { get; set; }    //t_czynnika4;

        public decimal HeatCapacity { get; set; } //pojemnosc cieplna 
        public decimal Density { get; set; }      //gęstość
    }
}
