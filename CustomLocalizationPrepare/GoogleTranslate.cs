using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CustomLocalizationPrepare {
  public class GoogleTranslate {
    public static string Translate(string src) {
      WebRequest request = WebRequest.Create("https://translation.googleapis.com/language/translate/v2?key=AIzaSyBP8Jr50K0HEuTU3XdBSV8qy_5zlCr6obM");
      request.Method = "POST";
      JObject jdata = new JObject();
      jdata["q"] = src;
      jdata["source"] = "en";
      jdata["target"] = "ru";
      jdata["format"] = "html";
      string postData = jdata.ToString();
      byte[] byteArray = Encoding.UTF8.GetBytes(postData);
      // Set the ContentType property of the WebRequest.
      request.ContentType = "application/json";
      // Set the ContentLength property of the WebRequest.
      request.ContentLength = byteArray.Length;
      // Get the request stream.
      Stream dataStream = request.GetRequestStream();
      // Write the data to the request stream.
      dataStream.Write(byteArray, 0, byteArray.Length);
      // Close the Stream object.
      dataStream.Close();
      // Get the response.
      WebResponse response = request.GetResponse();
      Console.WriteLine(((HttpWebResponse)response).StatusDescription);
      // Get the stream containing content returned by the server.
      dataStream = response.GetResponseStream();
      // Open the stream using a StreamReader for easy access.
      StreamReader reader = new StreamReader(dataStream);
      // Read the content.
      string responseFromServer = reader.ReadToEnd();
      // Display the content.
      Console.WriteLine(responseFromServer);
      // Clean up the streams.
      reader.Close();
      dataStream.Close();
      response.Close();
      JObject jResult = JObject.Parse(responseFromServer);
      return jResult["data"]["translations"][0]["translatedText"].ToString();
    }
  }
}
