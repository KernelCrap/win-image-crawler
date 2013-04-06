using System;

namespace Crawler.Interfaces
{
	public interface ICrawlerProcesserRules<in T>
	{
		bool IsValidUri(Uri uri);
		bool IsValid(T input);
	}
}