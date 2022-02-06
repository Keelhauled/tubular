using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Tubular
{
	[XmlRoot(ElementName="link", Namespace="http://www.w3.org/2005/Atom")]
	public class Link { 

		[XmlAttribute(AttributeName="rel", Namespace="")] 
		public string Rel { get; set; } = null!;

		[XmlAttribute(AttributeName="href", Namespace="")] 
		public string Href { get; set; } = null!;
	}

	[XmlRoot(ElementName="author", Namespace="http://www.w3.org/2005/Atom")]
	public class Author { 

		[XmlElement(ElementName="name", Namespace="http://www.w3.org/2005/Atom")] 
		public string Name { get; set; } = null!;

		[XmlElement(ElementName="uri", Namespace="http://www.w3.org/2005/Atom")] 
		public string Uri { get; set; } = null!;
	}

	[XmlRoot(ElementName="content", Namespace="http://search.yahoo.com/mrss/")]
	public class Content { 

		[XmlAttribute(AttributeName="url", Namespace="")] 
		public string Url { get; set; } = null!;

		[XmlAttribute(AttributeName="type", Namespace="")] 
		public string Type { get; set; } = null!;

		[XmlAttribute(AttributeName="width", Namespace="")] 
		public int Width { get; set; } 

		[XmlAttribute(AttributeName="height", Namespace="")] 
		public int Height { get; set; } 
	}

	[XmlRoot(ElementName="thumbnail", Namespace="http://search.yahoo.com/mrss/")]
	public class Thumbnail { 

		[XmlAttribute(AttributeName="url", Namespace="")] 
		public string Url { get; set; } = null!;

		[XmlAttribute(AttributeName="width", Namespace="")] 
		public int Width { get; set; } 

		[XmlAttribute(AttributeName="height", Namespace="")] 
		public int Height { get; set; } 
	}

	[XmlRoot(ElementName="starRating", Namespace="http://search.yahoo.com/mrss/")]
	public class StarRating { 

		[XmlAttribute(AttributeName="count", Namespace="")] 
		public int Count { get; set; } 

		[XmlAttribute(AttributeName="average", Namespace="")] 
		public double Average { get; set; } 

		[XmlAttribute(AttributeName="min", Namespace="")] 
		public int Min { get; set; } 

		[XmlAttribute(AttributeName="max", Namespace="")] 
		public int Max { get; set; } 
	}

	[XmlRoot(ElementName="statistics", Namespace="http://search.yahoo.com/mrss/")]
	public class Statistics { 

		[XmlAttribute(AttributeName="views", Namespace="")] 
		public int Views { get; set; } 
	}

	[XmlRoot(ElementName="community", Namespace="http://search.yahoo.com/mrss/")]
	public class Community { 

		[XmlElement(ElementName="starRating", Namespace="http://search.yahoo.com/mrss/")] 
		public StarRating StarRating { get; set; } = null!;

		[XmlElement(ElementName="statistics", Namespace="http://search.yahoo.com/mrss/")] 
		public Statistics Statistics { get; set; } = null!;
	}

	[XmlRoot(ElementName="group", Namespace="http://search.yahoo.com/mrss/")]
	public class Group { 

		[XmlElement(ElementName="title", Namespace="http://search.yahoo.com/mrss/")] 
		public string Title { get; set; } = null!;

		[XmlElement(ElementName="content", Namespace="http://search.yahoo.com/mrss/")] 
		public Content Content { get; set; } = null!;

		[XmlElement(ElementName="thumbnail", Namespace="http://search.yahoo.com/mrss/")] 
		public Thumbnail Thumbnail { get; set; } = null!;

		[XmlElement(ElementName="description", Namespace="http://search.yahoo.com/mrss/")] 
		public string Description { get; set; } = null!;

		[XmlElement(ElementName="community", Namespace="http://search.yahoo.com/mrss/")] 
		public Community Community { get; set; } = null!;
	}

	[XmlRoot(ElementName="entry", Namespace="http://www.w3.org/2005/Atom")]
	public class Entry { 

		[XmlElement(ElementName="id", Namespace="http://www.w3.org/2005/Atom")] 
		public string Id { get; set; } = null!;

		[XmlElement(ElementName="videoId", Namespace="http://www.youtube.com/xml/schemas/2015")] 
		public string VideoId { get; set; } = null!;

		[XmlElement(ElementName="channelId", Namespace="http://www.youtube.com/xml/schemas/2015")] 
		public string ChannelId { get; set; } = null!;

		[XmlElement(ElementName="title", Namespace="http://www.w3.org/2005/Atom")] 
		public string Title { get; set; } = null!;

		[XmlElement(ElementName="link", Namespace="http://www.w3.org/2005/Atom")] 
		public Link Link { get; set; } = null!;

		[XmlElement(ElementName="author", Namespace="http://www.w3.org/2005/Atom")] 
		public Author Author { get; set; } = null!;

		[XmlElement(ElementName="published", Namespace="http://www.w3.org/2005/Atom")] 
		public DateTime Published { get; set; } 

		[XmlElement(ElementName="updated", Namespace="http://www.w3.org/2005/Atom")] 
		public DateTime Updated { get; set; } 

		[XmlElement(ElementName="group", Namespace="http://search.yahoo.com/mrss/")] 
		public Group Group { get; set; } = null!;
	}

	[XmlRoot(ElementName="feed", Namespace="http://www.w3.org/2005/Atom")]
	public class Feed { 

		[XmlElement(ElementName="link", Namespace="http://www.w3.org/2005/Atom")] 
		public List<Link> Link { get; set; } = null!;

		[XmlElement(ElementName="id", Namespace="http://www.w3.org/2005/Atom")] 
		public string Id { get; set; } = null!;

		[XmlElement(ElementName="channelId", Namespace="http://www.youtube.com/xml/schemas/2015")] 
		public string ChannelId { get; set; } = null!;

		[XmlElement(ElementName="title", Namespace="http://www.w3.org/2005/Atom")] 
		public string Title { get; set; } = null!;

		[XmlElement(ElementName="author", Namespace="http://www.w3.org/2005/Atom")] 
		public Author Author { get; set; } = null!;

		[XmlElement(ElementName="published", Namespace="http://www.w3.org/2005/Atom")] 
		public DateTime Published { get; set; } 

		[XmlElement(ElementName="entry", Namespace="http://www.w3.org/2005/Atom")] 
		public List<Entry> Entry { get; set; } = null!;

		[XmlAttribute(AttributeName="yt", Namespace="http://www.w3.org/2000/xmlns/")] 
		public string Yt { get; set; } = null!;

		[XmlAttribute(AttributeName="media", Namespace="http://www.w3.org/2000/xmlns/")] 
		public string Media { get; set; } = null!;

		[XmlAttribute(AttributeName="xmlns", Namespace="")] 
		public string Xmlns { get; set; } = null!;

		[XmlText] 
		public string Text { get; set; } = null!;
	}
}
