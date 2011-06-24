//
//  Copyright (C) 2011 LShift Ltd.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using Net.LShift.Diffa.Participants.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Net.LShift.Diffa.Participants.Test {
  [TestFixture]
  public class BaseScanningParticipantTest {
    private static readonly string PARTICIPANT_URI = "http://localhost:16001/diffa/scan";
    private ServiceHost host; 

    //
    // NOTE: To run this test, you'll need to have previously run:
    //   netsh http add urlacl url=http://+:16001/ user="Users"
    // 
    // The above command authorizes the participant to bind on port 16001 and receive HTTP requests.
    //

    [SetUp]
    public void StartParticipant() {
      host = new ServiceHost(typeof(DummyScanningParticipant), new Uri(PARTICIPANT_URI));
      host.AddServiceEndpoint(typeof(IScanningParticipantContract), new WebHttpBinding(), "").Behaviors.Add(new WebHttpBehavior());
      host.Open();
    }

    [TearDown]
    public void TeardownParticipant() {
      host.Close();
    }

    [Test]
    public void ShouldRetrieveAllEntries() {
      var result = ExecuteScan("");
      var objs = SortById(result);

      var expected = new JArray(
          JObject.FromObject(new
          {
            id = "id1",
            version = "v1",
            attributes = new
            {
              bizDate = "2010-01-15",
              bookId = "1"
            }
          }),
          JObject.FromObject(new
          {
            id = "id2",
            version = "v2",
            attributes = new
            {
              bizDate = "2010-01-16",
              bookId = "1"
            }
          }),
          JObject.FromObject(new
          {
            id = "id3",
            version = "v3",
            attributes = new
            {
              bizDate = "2010-01-15",
              bookId = "2"
            }
          }),
          JObject.FromObject(new
          {
            id = "id4",
            version = "v4",
            attributes = new
            {
              bizDate = "2010-01-15",
              bookId = "1"
            }
          })
        );

      AssertJsonEqual(expected, objs);

      Assert.IsTrue(new[] { "id1", "id2", "id3", "id4" }.SequenceEqual(objs.Select(o => (string)((JObject)o)["id"])));
    }

    [Test]
    public void ShouldBucketEntriesByDate() {
      var result = ExecuteScan("bizDate-granularity=daily");
      var objs = SortByBizDateAndBook(result);

      var expected = new JArray(
          JObject.FromObject(new {
            version = DigestUtils.Md5Hex("v1" + "v4"),
            attributes = new {
              bizDate = "2010-01-15",
              bookId = "1"
            }
          }),
          JObject.FromObject(new
          {
            version = DigestUtils.Md5Hex("v3"),
            attributes = new
            {
              bizDate = "2010-01-15",
              bookId = "2"
            }
          }),
          JObject.FromObject(new {
            version = DigestUtils.Md5Hex("v2"),
            attributes = new {
              bizDate = "2010-01-16",
              bookId = "1"
            }
          })
        );

      AssertJsonEqual(expected, objs);
    }

    [Test]
    public void ShouldFilterEntriesByBook()
    {
      var result = ExecuteScan("bookId=1");
      var objs = SortById(result);

      var expected = new JArray(
          JObject.FromObject(new
          {
            id = "id1",
            version = "v1",
            attributes = new
            {
              bizDate = "2010-01-15",
              bookId = "1"
            }
          }),
          JObject.FromObject(new
          {
            id = "id2",
            version = "v2",
            attributes = new
            {
              bizDate = "2010-01-16",
              bookId = "1"
            }
          }),
          JObject.FromObject(new
          {
            id = "id4",
            version = "v4",
            attributes = new
            {
              bizDate = "2010-01-15",
              bookId = "1"
            }
          })
        );

      AssertJsonEqual(expected, objs);
    }

    [Test]
    public void ShouldAggregateAndFilter()
    {
      var result = ExecuteScan("bookId=1&bizDate-granularity=daily");
      var objs = SortByBizDateAndBook(result);

      var expected = new JArray(
          JObject.FromObject(new
          {
            version = DigestUtils.Md5Hex("v1" + "v4"),
            attributes = new
            {
              bizDate = "2010-01-15",
              bookId = "1"
            }
          }),
          JObject.FromObject(new
          {
            version = DigestUtils.Md5Hex("v2"),
            attributes = new
            {
              bizDate = "2010-01-16",
              bookId = "1"
            }
          })
        );

      AssertJsonEqual(expected, objs);
    }

    private static JArray ExecuteScan(string queryString) {
      var req = (HttpWebRequest) HttpWebRequest.Create(PARTICIPANT_URI + "/" + (!string.IsNullOrEmpty(queryString) ? "?" + queryString : ""));
      var resp = req.GetResponse() as HttpWebResponse;

      if (resp.StatusCode != HttpStatusCode.OK)
        throw new Exception(string.Format("Unexpected non-200 response: {0}", resp.StatusCode));

      return JArray.Parse(new StreamReader(resp.GetResponseStream()).ReadToEnd());
    }

    private static JArray SortById(IEnumerable<JToken> objs) {
      var result = objs.ToArray();
      Array.Sort(result, (o1, o2) =>
      {
        var o1Id = (string)o1["id"];
        var o2Id = (string)o2["id"];
        return o1Id.CompareTo(o2Id);
      });
      return new JArray(result);
    }

    private static JArray SortByBizDateAndBook(IEnumerable<JToken> objs) {
      var result = objs.ToArray();
      Array.Sort(result, (o1, o2) =>
      {
        var bizDate1 = (string)o1["attributes"]["bizDate"];
        var bizDate2 = (string)o2["attributes"]["bizDate"];
        var bookId1 = (string)o1["attributes"]["bookId"];
        var bookId2 = (string)o2["attributes"]["bookId"];

        var bizDateRes = bizDate1.CompareTo(bizDate2);
        if (bizDateRes == 0) return bookId1.CompareTo(bookId2);
        return bizDateRes;
      });

      return new JArray(result);
    }

    private static void AssertJsonEqual(JArray expected, JArray actual) {
      Assert.AreEqual(expected.ToString(), actual.ToString(), string.Format("{0} is not the same as {1}", expected, actual));
    }
  }

  public class DummyScanningParticipant : BaseScanningParticipant {
    private List<DummyEntity> _data = new List<DummyEntity> {
      new DummyEntity { Id = "id1", BizDate = new DateTime(2010, 1, 15), BookId = "1", Version = "v1" },
      new DummyEntity { Id = "id2", BizDate = new DateTime(2010, 1, 16), BookId = "1", Version = "v2" },
      new DummyEntity { Id = "id3", BizDate = new DateTime(2010, 1, 15), BookId = "2", Version = "v3" },
      new DummyEntity { Id = "id4", BizDate = new DateTime(2010, 1, 15), BookId = "1", Version = "v4" }
    };

    protected override IEnumerable<ICategoryFunction> DetermineAggregations(System.Collections.Specialized.NameValueCollection requestParams) {
      var builder = new AggregationBuilder(requestParams);
      builder.MaybeAddDateAggregation("bizDate");
      builder.MaybeAddByNameAggregation("bookId");
      return builder.ToList();
    }

    protected override IEnumerable<IQueryConstraint> DetermineConstraints(System.Collections.Specialized.NameValueCollection requestParams) {
      var builder = new ConstraintsBuilder(requestParams);
      builder.MaybeAddDateRangeConstraint("bizDate");
      builder.MaybeAddSetConstraint("bookId");
      return builder.ToList();
    }

    protected override IEnumerable<ScanResultEntry> Query(IEnumerable<IQueryConstraint> constraints, IEnumerable<ICategoryFunction> aggregations) {
      var constrainedData = _data.Where(d => d.Satisfies(constraints));

      if (aggregations.Count() > 0) {
        var digester = new DigestBuilder(aggregations);
        foreach (var d in constrainedData) digester.Add(d.Id, d.Attributes, d.Version);
        return digester.GetScanResults().ToArray();
      } else {
        return constrainedData.Select(d => new ScanResultEntry(d.Id, d.Attributes, null, d.Version));
      }
    }
  }

  class DummyEntity {
    public string Id { get; set; }
    public DateTime BizDate { get; set; }
    public string BookId { get; set; }
    public string Version { get; set; }

    public IDictionary<string, string> Attributes {
      get { return new Dictionary<string, string> {{"bizDate", BizDate.ToString("yyyy-MM-dd")}, {"bookId", BookId}}; }
    }

    public bool Satisfies(IEnumerable<IQueryConstraint> constraints) {
      foreach (var c in constraints) {
        switch (c.Category) {
          case "bizDate":
            if (!((DateRangeQueryConstraint)c).Includes(BizDate)) return false;
            break;
          case "bookId":
            if (!((SetQueryConstraint)c).Includes(BookId)) return false;
            break;
          default:
            throw new ArgumentException(string.Format("Unrecognised query constraint: {0}", c));
        }
      }

      return true;
    }
  }
}
