using System;
using System.Web;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NVelocity;
using NVelocity.App;
using NVelocity.Context;
using NVelocity.App.Tools;
using NVelocity.Runtime;
using Commons.Collections;

namespace MyDBAssistant
{
    /// <summary>
    ///  NVelocity模板工具类 VelocityHelper
    /// </summary>
    public class VelocityHelper
    {
        private VelocityEngine _velocity = null;
        private IContext _context = null;

        public VelocityHelper()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public VelocityHelper(string templatePath)
        {
            Init(templatePath);
        }

        /// <summary>
        /// 初始话NVelocity模块
        /// </summary>
        /// <param name="templatDir">模板文件夹路径</param>
        public void Init(string templatDir)
        {
            //创建VelocityEngine实例对象
            _velocity = new VelocityEngine();
            //使用设置初始化VelocityEngine
            ExtendedProperties props = new ExtendedProperties();
            props.AddProperty(RuntimeConstants.RESOURCE_LOADER, "file");
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, templatDir);
            props.AddProperty(RuntimeConstants.INPUT_ENCODING, "utf-8");
            props.AddProperty(RuntimeConstants.OUTPUT_ENCODING, "utf-8");
            _velocity.Init(props);

            //为模板变量赋值
            _context = new VelocityContext();
            _context.Put("formatter", new VelocityFormatter(_context));
        }

        /// <summary>
        /// 给模板变量赋值
        /// </summary>
        /// <param name="key">模板变量</param>
        /// <param name="value">模板变量值</param>
        public void PutSet(string key, object value)
        {
            if (_context == null)
            {
                _context = new VelocityContext();
            }           
            _context.Put(key, value);
        }

        /// <summary>
        /// 显示模板
        /// </summary>
        /// <param name="templatFileName">模板文件名</param>
        public string Display(string templatFileName)
        {
            //从文件中读取模板
            //Template template = velocity.GetTemplate(templatFileName);
            Template template = _velocity.GetTemplate(templatFileName, "UTF-8");
            //合并模板
            StringWriter writer = new StringWriter();
            template.Merge(_context, writer);
            //输出
            //HttpContext.Current.Response.Clear();
            //HttpContext.Current.Response.Write(writer.ToString());
            //HttpContext.Current.Response.Flush();
            //HttpContext.Current.Response.End();
            string text = writer.ToString();
            return text;
        }

        #region 使用方法:
        /*
        VelocityHelper vh = new VelocityHelper();
        vh.Init(@"templates");//指定模板文件的相对路径
        vh.PutSet("title", "员工信息");
        vh.PutSet("comName","成都xxxx里公司");
        vh.PutSet("property”,"天营");
        ArrayList aems = new ArrayList();
        //使用tp1.htm模板显示
        vh.Display("tp1.htm");
        */
        #endregion

    }
}
