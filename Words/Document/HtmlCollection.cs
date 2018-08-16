using System.Collections;
using System.Collections.Generic;
using AngleSharp.Dom;
using System.Linq;
using System.Text;

namespace RussianWordScraper.Document
{

    public class HtmlCollection : List<IElement>, IHtmlCollection<IElement>
    {
        public IElement this[string id]
        {
            get
            {
                return this.First(item => item.Id == id);
            }
        }

        public int Length
        {
            get
            {
                return Count;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public HtmlCollection QuerySelectorAll(string selector)
        {
            if (string.IsNullOrWhiteSpace(selector))
            {
                return this;
            }
            var collection = new HtmlCollection();
            collection.AddRange(_querySelectorAll(selector));
            return collection;
        }
        public HtmlCollection Clone(bool deep = true)
        {
            var collection = new HtmlCollection();
            foreach (var element in this)
                collection.Add(element.Clone(deep) as IElement);
            return collection;
        }

        IEnumerable<IElement> _querySelectorAll(string selector)
        {
            foreach (var item in this)
            {
                foreach (var element in item.QuerySelectorAll(selector))
                    yield return element;
            }
        }



        public string ToHtml()
        {
            var builder = new StringBuilder("");
            this.ForEach(item => builder.Append(item.OuterHtml));
            return builder.ToString();

        }
    }
}
