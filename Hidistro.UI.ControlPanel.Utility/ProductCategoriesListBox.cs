namespace Hidistro.UI.ControlPanel.Utility
{
    using Hidistro.ControlPanel.Commodities;
    using Hidistro.Core;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using Hidistro.Entities.Commodities;

    public class ProductCategoriesListBox : ListBox
    {
        private string strDepth = "　　";

        public override void DataBind()
        {
            this.Items.Clear();
            IList<CategoryInfo> sequenceCategories = CatalogHelper.GetSequenceCategories();
            for (int i = 0; i < sequenceCategories.Count; i++)
            {
                this.Items.Add(new ListItem(this.FormatDepth(sequenceCategories[i].Depth, Globals.HtmlDecode(sequenceCategories[i].Name)), sequenceCategories[i].CategoryId.ToString(CultureInfo.InvariantCulture)));
            }
            ListItem item = new ListItem("--所有--", "0");
            this.Items.Insert(0, item);
        }

        private string FormatDepth(int depth, string categoryName)
        {
            for (int i = 1; i < depth; i++)
            {
                categoryName = this.strDepth + categoryName;
            }
            return categoryName;
        }

        public int SelectedCategoryId
        {
            get
            {
                int num = 0;
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].Selected)
                    {
                        num = int.Parse(this.Items[i].Value);
                    }
                }
                return num;
            }
            set
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].Value == value.ToString())
                    {
                        this.Items[i].Selected = true;
                    }
                    else
                    {
                        this.Items[i].Selected = false;
                    }
                }
            }
        }

        public IList<int> SelectedValue
        {
            get
            {
                IList<int> list = new List<int>();
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].Selected)
                    {
                        list.Add(int.Parse(this.Items[i].Value));
                    }
                }
                return list;
            }
            set
            {
                int num = 0;
                while (num < this.Items.Count)
                {
                    this.Items[num].Selected = false;
                    num++;
                }
                IList<int> list = value;
                foreach (int num2 in list)
                {
                    for (num = 0; num < this.Items.Count; num++)
                    {
                        if (this.Items[num].Value == num2.ToString())
                        {
                            this.Items[num].Selected = true;
                        }
                    }
                }
            }
        }
    }
}

