using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Network;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Words;
using Words.Document;

namespace RussianWordScraper.Document
{
    public class Composition
    {
        public string Name { get; set; }
        public ContentSegment Return { get; set; }
    }



    public class ContentSegment
    {
        private readonly IRequester _requester;
        public ContentSegment(IRequester requester) {
            _requester = requester;
        }
        public string Name = Guid.NewGuid().ToString();
        public string Url { get; set; }
        public string Select { get; set; }
        public string Raw { get; set; }
        public List<Action> Actions { get; set; }

        public async Task<HtmlCollection> DocumentElement()
        {
            IDocument container;
            if (!string.IsNullOrWhiteSpace(Raw))
            {
                var parser = new HtmlParser();
                container = parser.Parse(Raw);
            }
            else if (!string.IsNullOrWhiteSpace(Url))
            {
                container = await BrowsingContext.New(Configuration.Default.WithDefaultLoader(requesters: new[] { _requester })).OpenAsync(Url);
            }
            else
                throw new Exception("no source");

            if (Actions != null)
                foreach (var action in Actions)
                {
                    var subjects = container.QuerySelectorAll(action.At);
                    if (action.Type == "insert")
                    {
                        var sources = await action.Source.DocumentElement();
                        subjects.ForEach(subject => sources.ForEach(source => subject.AppendChild(source.Clone(deep: true))));
                    }
                    else if (action.Type == "replace")
                    {
                        var sources = await action.Source.DocumentElement();
                        subjects.ForEach(subject => subject.Replace(sources.Clone().ToArray()));
                    }
                    else if (action.Type == "remove")
                    {
                        subjects.ForEach(subject => subject.Remove());
                    }
                    else if (action.Type == "insert-after")
                    {
                        var sources = await action.Source.DocumentElement();
                        subjects.ForEach(subject => subject.Insert(AdjacentPosition.AfterEnd, sources.ToHtml()));
                    }
                    else if (action.Type == "insert-before")
                    {
                        var sources = await action.Source.DocumentElement();
                        subjects.ForEach(subject => subject.Insert(AdjacentPosition.BeforeBegin, sources.ToHtml()));
                    }
                    else if (action.Type == "wrap")
                    {
                        var sources = await action.Source.DocumentElement();
                        if (sources.Count > 1)
                            throw new Exception("Wrap can only support a single element in the source");
                        subjects.ForEach(subject => {
                            var contextSource = sources.First().Clone();
                            subject.Replace(contextSource);
                            contextSource.AppendChild(subject);
                        });
                    }
                }

            var elements = new HtmlCollection();
            if (!string.IsNullOrWhiteSpace(Raw))
            {
                if (Raw.Contains("<html>"))
                {
                    elements.Add(container.DocumentElement);
                }
                else
                {
                    if (container.Body.Children.Any())
                    {
                        elements.AddRange(container.Body.Children);
                    }
                    else if (container.Head.Children.Any())
                    {
                        elements.AddRange(container.Head.Children);
                    }
                    else
                        throw new Exception("Could not find created element");
                }

                if (!string.IsNullOrWhiteSpace(Select))
                {
                    elements = elements.QuerySelectorAll(Select);
                }

            }
            else if (!string.IsNullOrWhiteSpace(Url))
            {
                if (!string.IsNullOrWhiteSpace(Select))
                {
                    elements.AddRange(container.QuerySelectorAll(Select));
                }
                else
                {
                    elements.Add(container.DocumentElement);
                }
            }
            else
                throw new Exception("no source");


            return elements;
        }
        private IHtmlCollection<IElement> getAllSubjects(IElement element, string selector)
        {
            if (element is IHtmlHtmlElement)
            {
                return element.QuerySelectorAll(selector);
            }
            return element.ParentElement.QuerySelectorAll(selector);
        }
        private IElement getSubject(IElement element, string selector)
        {
            if (element is IHtmlHtmlElement)
            {
                return element.QuerySelector(selector);
            }
            return element.ParentElement.QuerySelector(selector);
        }

    }

    public class Action
    {
        public string Type { get; set; }
        public string At { get; set; }
        public ContentSegment Source { get; set; }
    }

    public static class Extensions
    {
        public static void ForEach(this IHtmlCollection<IElement> elements, Action<IElement> action)
        {
            foreach (var element in elements) action(element);
        }
    }
}
