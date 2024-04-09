using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;

namespace UnitTestProject1
{
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            string webhook = "{\"data\":{\"payload\":{\"event\":\"media.pause\",\"user\":true,\"owner\":true,\"Account\":{\"id\":16058768,\"thumb\":\"https://plex.tv/users/c5f17e36bc1fa3b6/avatar?c=1712653758\",\"title\":\"Stormwind366\"},\"Server\":{\"title\":\"AERO\",\"uuid\":\"55343b3d50976f60266faada39b2a1da9d801940\"},\"Player\":{\"local\":true,\"publicAddress\":\"81.78.52.245\",\"title\":\"ED-DESKTOP-11\",\"uuid\":\"r6tspbu43kf7wbvuoeg0k4zd\"},\"Metadata\":{\"librarySectionType\":\"artist\",\"ratingKey\":\"90995\",\"key\":\"/library/metadata/90995\",\"parentRatingKey\":\"90994\",\"grandparentRatingKey\":\"16741\",\"guid\":\"plex://track/5d07cf7a403c6402901d435d\",\"parentGuid\":\"plex://album/5d07c435403c6402909d95fc\",\"grandparentGuid\":\"plex://artist/5d07bbfc403c6402904a5ef4\",\"parentStudio\":\"Columbia\",\"type\":\"track\",\"title\":\"Shoot to Thrill\",\"grandparentKey\":\"/library/metadata/16741\",\"parentKey\":\"/library/metadata/90994\",\"librarySectionTitle\":\"Music\",\"librarySectionID\":3,\"librarySectionKey\":\"/library/sections/3\",\"grandparentTitle\":\"AC/DC\",\"parentTitle\":\"Iron Man 2\",\"summary\":\"\",\"index\":1,\"parentIndex\":1,\"ratingCount\":748610,\"viewCount\":7,\"lastViewedAt\":1712674377,\"parentYear\":2010,\"thumb\":\"/library/metadata/90994/thumb/1710059509\",\"art\":\"/library/metadata/16741/art/1712036807\",\"parentThumb\":\"/library/metadata/90994/thumb/1710059509\",\"grandparentThumb\":\"/library/metadata/16741/thumb/1712036807\",\"grandparentArt\":\"/library/metadata/16741/art/1712036807\",\"addedAt\":1706352446,\"updatedAt\":1710059509,\"Genre\":[{\"id\":78768,\"filter\":\"genre=78768\",\"tag\":\"Pop/Rock\"}],\"Guid\":[{\"id\":\"mbid://889c8132-5f97-42bf-9908-99e7c5027cc4\"}],\"Mood\":[{\"id\":78802,\"filter\":\"mood=78802\",\"tag\":\"Rousing\"},{\"id\":79118,\"filter\":\"mood=79118\",\"tag\":\"Rebellious\"},{\"id\":82057,\"filter\":\"mood=82057\",\"tag\":\"Tough\"},{\"id\":79004,\"filter\":\"mood=79004\",\"tag\":\"Fun\"},{\"id\":79180,\"filter\":\"mood=79180\",\"tag\":\"Snide\"},{\"id\":79125,\"filter\":\"mood=79125\",\"tag\":\"Raucous\"}]}}},\"files\":[]}";
            JObject o = JObject.Parse(webhook);

            string jsonPath = "$..[?(@.event=~/media\\..+/)]";
            JToken jToken = o.SelectToken(jsonPath);

            Assert.That(jToken, Is.Not.Null);
        }
    }
}
