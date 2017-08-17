using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDBAssistant.Schema
{
    public class CurrentRow
    {
        /// <summary>
        /// 滚动条所在位置中的第一行
        /// </summary>
        public int ScrollingIndex { get; set; }

        /// <summary>
        /// 选中的行号
        /// </summary>
        public int SelectIndex { get; set; }

        public CurrentRow()
        {
            this.ScrollingIndex = 0;
            this.SelectIndex = 0;
        }
    }
}
