namespace TC421
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class ModelItem
    {
        private List<object> _ModelValues;

        public ModelItem()
        {
            int num = 0;
            for (int index = 0; index < 288; ++index)
            {
                if (index % 6 == 0)
                {
                    this.ModelValues.Add((object)(string.Format("{0}", (object)(num / 60).ToString().PadLeft(2, '0')) + string.Format(":{0}", (object)(num % 60).ToString().PadLeft(2, '0'))));
                    num += 30;
                }
                else
                    this.ModelValues.Add((object)0);
            }
        }

        public string ModelItemName { set; get; }

        public List<object> ModelValues
        {
            set => this._ModelValues = value;
            get => this._ModelValues ?? (this._ModelValues = new List<object>(288));
        }

        public void ModelValueInt()
        {
            this.ModelValues.Clear();
            int num1 = 0;

            for (int index = 0; index < 288; ++index)
            {
                int num2 = index % 6;
                if (num2 != 0) this.ModelValues.Add((object)0);
                else 
                {
                    List<object> modelValues = this.ModelValues;
                    string str1 = string.Format("{0}", (object)(num1 / 60).ToString().PadLeft(2, '0'));
                    string str2 = string.Format(":{0}", (object)(num1 % 60).ToString().PadLeft(2, '0'));
                    modelValues.Add((object)str1 + str2);
                    num1 += 30;
                }
            }
        }
    }
}
