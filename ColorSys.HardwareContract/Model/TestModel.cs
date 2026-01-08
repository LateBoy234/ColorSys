using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract.Model
{
    public  class TestModel
    {
        public TestModel()
        {
            DateTime= DateTime.Now;
        }
        public TestModel(TestModel test)
        {
            ID=test.ID;
            Name=test.Name;
            DateTime=test.DateTime;
            Material=test.Material;
            OpticalStruct=test.OpticalStruct;
            InstrumentSN=test.InstrumentSN;
            DataValues=test.DataValues;
        }
        
        /// <summary>
        /// 样品ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 样品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 测量时间
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 料号
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// 光学结构
        /// </summary>
        public string OpticalStruct { get; set; }
        /// <summary>
        /// 仪器的编号
        /// </summary>
        public string InstrumentSN { get; set; }
        /// <summary>
        /// 结果值
        /// </summary>
       public  double[] DataValues {  get; set; }


    }
}
