using FlyingFive.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Tests.Entities
{
    /// <summary>
    /// 菜品资料
    /// </summary>
    [Serializable]
    public class Dish //: SISS.Core.Data.BaseEntity
    {
        /// <summary>
        /// 编码
        /// </summary>
        public virtual string DishNo { get; set; }
        /// <summary>
        /// 辅助码
        /// </summary>
        public virtual string SubNo { get; set; }
        /// <summary>
        /// 品名
        /// </summary>
        public virtual string DishName { get; set; }
        /// <summary>
        /// 拼音简码
        /// </summary>
        public virtual string Spell { get; set; }
        /// <summary>
        /// 英文品名
        /// </summary>
        public virtual string English { get; set; }
        /// <summary>
        /// 所属大类
        /// </summary>
        public virtual string SeriesNo { get; set; }
        /// <summary>
        /// 所属小类
        /// </summary>
        public virtual string TypeNo { get; set; }
        /// <summary>
        /// 销售单位
        /// </summary>
        public virtual string UnitNo { get; set; }
        /// <summary>
        /// 销售规格
        /// </summary>
        public virtual string Spec { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public virtual decimal? Price1 { get; set; }
        /// <summary>
        /// 是否停用
        /// </summary>
        public virtual string StopFlag { get; set; }
        /// <summary>
        /// 是否套餐
        /// </summary>
        public virtual string SuitFlag { get; set; }
        /// <summary>
        /// 是否时价
        /// </summary>
        public virtual string CurFlag { get; set; }
        /// <summary>
        /// 是否可折扣
        /// </summary>
        public virtual string Discount { get; set; }
        /// <summary>
        /// 是否可下载到点菜机
        /// </summary>
        public virtual string DownFlag { get; set; }
        /// <summary>
        /// 菜品说明
        /// </summary>
        public virtual string Explain { get; set; }
        /// <summary>
        /// 点菜提成
        /// </summary>
        public virtual string DishFlag { get; set; }
        /// <summary>
        /// 点菜提成方式(1:按比率,2:按金额)
        /// </summary>
        public virtual string DeductType { get; set; }
        /// <summary>
        /// 点菜定额提成额
        /// </summary>
        public virtual decimal? DeductAmount { get; set; }
        /// <summary>
        /// 点菜比率提成
        /// </summary>
        public virtual decimal? DeductRate { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime Build { get; set; }
        /// <summary>
        /// 点菜宝编码
        /// </summary>
        public virtual string DcbCode { get; set; }
        /// <summary>
        /// 是否直销商品
        /// </summary>
        public virtual string ItemFlag { get; set; }
        /// <summary>
        /// 对应原料
        /// </summary>
        public virtual string MaterialNo { get; set; }
        /// <summary>
        /// 偶数半价
        /// </summary>
        public virtual string HalfFlag { get; set; }
        /// <summary>
        /// 外卖价
        /// </summary>
        public virtual decimal? OutPrice { get; set; }
        /// <summary>
        /// 是否称重菜品
        /// </summary>
        public virtual string Electscale { get; set; }
        ///// <summary>
        ///// 是否计入低消
        ///// </summary>
        //public virtual string Ch_lowflag { get; set; }
        ///// <summary>
        ///// 是否收服务费
        ///// </summary>
        //public virtual string Ch_serviceflag { get; set; }
    }

    public class DishMap : EntityMappingConfiguration<Dish>
    {
        public DishMap()
        {
            this.Table("BD_Dish");

            this.Property(entity => entity.DishNo).HasColumnName("ch_dishno").HasMaxSize(7).HasPrimaryKey().AddToEntityMapping();
            this.Property(entity => entity.SubNo).HasColumnName("vch_subno").HasRequired(false).HasMaxSize(20).AddToEntityMapping();
            this.Property(entity => entity.DishName).HasColumnName("vch_dishname").HasRequired(false).HasMaxSize(30).AddToEntityMapping();
            this.Property(entity => entity.Spell).HasColumnName("vch_spell").HasRequired(false).HasMaxSize(15).AddToEntityMapping();
            this.Property(entity => entity.English).HasColumnName("vch_english").HasRequired(false).HasMaxSize(40).AddToEntityMapping();
            this.Property(entity => entity.SeriesNo).HasColumnName("ch_seriesno").HasRequired(false).HasMaxSize(2).AddToEntityMapping();
            this.Property(entity => entity.TypeNo).HasColumnName("ch_typeno").HasRequired(false).HasMaxSize(4).AddToEntityMapping();
            this.Property(entity => entity.UnitNo).HasColumnName("ch_unitno").HasRequired(false).HasMaxSize(3).AddToEntityMapping();
            this.Property(entity => entity.Spec).HasColumnName("vch_spec").HasRequired(false).HasMaxSize(20).AddToEntityMapping();
            this.Property(entity => entity.Price1).HasColumnName("num_price1").HasRequired(false).HasMaxSize(9).AddToEntityMapping();
            this.Property(entity => entity.StopFlag).HasColumnName("ch_stopflag").HasRequired(false).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.SuitFlag).HasColumnName("ch_suitflag").HasRequired(false).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.CurFlag).HasColumnName("ch_curflag").HasRequired(false).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.Discount).HasColumnName("ch_discount").HasRequired(false).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.DownFlag).HasColumnName("ch_downflag").HasRequired(false).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.Explain).HasColumnName("vch_explain").HasRequired(false).HasMaxSize(254).AddToEntityMapping();
            this.Property(entity => entity.DishFlag).HasColumnName("ch_dishflag").HasRequired(false).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.DeductType).HasColumnName("ch_dishtype").HasRequired(false).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.DeductAmount).HasColumnName("num_dish_deduct").HasRequired(false).HasMaxSize(9).AddToEntityMapping();
            this.Property(entity => entity.DeductRate).HasColumnName("int_dish_deduct").HasRequired(false).HasMaxSize(9).AddToEntityMapping();
            this.Property(entity => entity.Build).HasColumnName("dt_build").HasRequired(false).HasMaxSize(5).AddToEntityMapping();
            this.Property(entity => entity.DcbCode).HasColumnName("vch_dcb_code").HasRequired(false).HasMaxSize(5).AddToEntityMapping();
            this.Property(entity => entity.ItemFlag).HasColumnName("ch_itemflag").HasRequired(false).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.MaterialNo).HasColumnName("ch_materialno").HasRequired(false).HasMaxSize(7).AddToEntityMapping();
            this.Property(entity => entity.HalfFlag).HasColumnName("ch_halfflag").HasRequired(true).HasMaxSize(1).AddToEntityMapping();
            this.Property(entity => entity.OutPrice).HasColumnName("num_outprice").HasRequired(false).HasMaxSize(9).AddToEntityMapping();
            this.Property(entity => entity.Electscale).HasColumnName("ch_electscale").HasRequired(true).HasMaxSize(1).AddToEntityMapping();
            this.AddToEntityMapping();
        }
    }
}
