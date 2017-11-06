namespace Hidistro.UI.ControlPanel.Utility
{
    using Hidistro.ControlPanel.Commodities;
    using Hidistro.Entities.Commodities;
    using System;
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    public class ProductTypesCheckBoxList : CheckBoxList
    {
        private int repeatColumns = 7;
        private System.Web.UI.WebControls.RepeatDirection repeatDirection = System.Web.UI.WebControls.RepeatDirection.Horizontal;

        public override void DataBind()
        {
            this.Items.Clear();
            IList<ProductTypeInfo> productTypes = ProductTypeHelper.GetProductTypes();
            foreach (ProductTypeInfo info in productTypes)
            {
                base.Items.Add(new ListItem(info.TypeName, info.TypeId.ToString()));
            }
        }

        public override int RepeatColumns
        {
            get
            {
                return this.repeatColumns;
            }
            set
            {
                this.repeatColumns = value;
            }
        }

        public override System.Web.UI.WebControls.RepeatDirection RepeatDirection
        {
            get
            {
                return this.repeatDirection;
            }
            set
            {
                this.repeatDirection = value;
            }
        }
    }
}

