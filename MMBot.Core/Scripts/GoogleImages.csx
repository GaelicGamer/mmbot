/**
* <description>
*     Pulls an image from the google images api
* </description>
*
* <commands>
*     mmbot image me &lt;query&gt; - Returns an image matching the requested search term.
* </commands>
* 
* <notes>
*     Ported from https://github.com/github/hubot/blob/master/src/scripts/google-images.coffee
* </notes>
* 
* <author>
*     PeteGoo
* </author>
*/

using System.Text.RegularExpressions;

var robot = Require<Robot>();

Random _random = new Random(DateTime.Now.Millisecond);
Regex _httpRegex = new Regex(@"^https?:\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

robot.Respond(@"(image|img)( me)? (.*)", msg => ImageMe(msg, msg.Match[3], url => msg.Send(url)));

robot.Respond(@"animate( me)? (.*)", msg => ImageMe(msg, msg.Match[2], url => msg.Send(url), true));

robot.Respond(@"(?:mo?u)?sta(?:s|c)he?(?: me)? (.*)", msg =>
{
    var type = _random.Next(2);
    var mustachify = string.Format("http://mustachify.me/{0}?src=", type);
    var imagery = msg.Match[1];
    if (_httpRegex.IsMatch(imagery))
    {
        msg.Send(mustachify + imagery);
    }
    else
    {
        ImageMe(msg, imagery, url => msg.Send(mustachify + url), false, true);
    }
                
});

private void ImageMe(IResponse<TextMessage> msg, string query, Action<string> cb, bool animated = false, bool faces = false )
{
	msg.Http("https://www.googleapis.com/customsearch/v1")
        .Query(new
        {
            q = query,
            cx = "put cx here",
			num = 5,
            safe = "high",
            fileType = animated ? "gif" : null,
            hq = animated ? "animated" : null,
            imgType = faces ? "face" : null,
			key = "put key here",
        })
        .GetJson((err, res, body) => {
            try
            {			
				//Only works for a single image atm.
                var images = body["items"].ToArray();
                if (images.Count() > 0)
                {
                    var image = msg.Random(images);
                    cb(string.Format("{0}", image["pagemap/cse_image/src"]));
					
					string imageURL = image["pagemap"]["cse_image"][0].ToString();
					
					//Strips the preceding and trailing formatting because its a JSON src that we can't access for some reason.
					imageURL = imageURL.Substring(13,imageURL.Length-13);
					imageURL = imageURL.Substring(0,imageURL.Length-4);
					
					//msg.Send("Success sir!");
					msg.Send(imageURL);
                }
                else
                {
                    msg.Send("I tried hard sir, but there just wasn't anything I could find for " + query);
                }
            }
            catch (Exception ex)
            {
                msg.Send("Error: " + ex.Message + ex.StackTrace);
            }
    });
}
