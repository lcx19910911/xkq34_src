namespace Hidistro.UI.ControlPanel.Utility
{
    using Hidistro.ControlPanel.Members;
    using Hidistro.Core;
    using Hidistro.Entities.Members;
    using System;
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    public class MemberGradeCheckBoxList : CheckBoxList
    {
        public override void DataBind()
        {
            this.Items.Clear();
            IList<MemberGradeInfo> memberGrades = MemberHelper.GetMemberGrades();
            int num = 0;
            foreach (MemberGradeInfo info in memberGrades)
            {
                this.Items.Add(new ListItem(Globals.HtmlDecode(info.Name), info.GradeId.ToString()));
                this.Items[num++].Selected = true;
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

