using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HtmlAgilityPack;

namespace QueryCollaboratorUsers
{
  class Program
  {
    // how many users can be online at the same time, if exceeds this number, then the exe will logoff the users
    private static readonly string s_maxConcurrentCountKey = ConfigurationManager.AppSettings["MaxConcurrentCount"];

    private static List<User> _activeUsers = new List<User>();
    private static int _maxOnlineUser;

    // the user inactive for 20 minutes will be logoff
    private const double InactiveDurationThresholdInSeconds = 20 * 60;

    static void Main(string[] args)
    {
      // only add log on 00:00
      if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0)
      {
        WriteLogs($"\r\n{DateTime.Now}");
      }

      var parseResult = int.TryParse(s_maxConcurrentCountKey, out _maxOnlineUser);
      if (!parseResult)
      {
        Console.WriteLine($"The config value for max concurrent license user is wrong {s_maxConcurrentCountKey}, please set a int value which is less than 6.");
      }

      var activeUserPageHtmlString = GetActiveUserPage();
      ParseActiveUserPage(activeUserPageHtmlString);

      var logoffUsersList = LogoffUsersByInactiveDuration();
      var forceLogoffList = LogoffUsersByOrder();

      logoffUsersList.AddRange(forceLogoffList);

      if (logoffUsersList.Any())
      {
        AddLogsForLogoffUsers(logoffUsersList);
      }

      LogoffMyself();
      
      // sleeps to let us see the console outputs
      System.Threading.Thread.Sleep(5000);
      Environment.Exit(0);
    }

    private static string GetActiveUserPage()
    {
      // get active users
      var proc = new Process
      {
        StartInfo =
        {
          FileName = "C:\\Program Files\\Collaborator Client\\ccollab.exe",
          UseShellExecute = false,
          RedirectStandardInput = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          CreateNoWindow = true,
          Arguments = $"admin wget \"go?formSubmitteduserOpts=1&collaborator.security.token=181053ce70e556b66cd25d6e56b4d981&pv_component=ErrorsAndMessages&pv_pageNumber=1&pv_itemsPerPage=100&pv_step=AdminUsers&page=Admin&pv_ErrorsAndMessages_fingerPrint=861101&offsetX=0&offsetY=0&userListSort=LOGIN_ASC&userListFilter=SHOW_ACTIVE_PER_LAST_HOUR&searchByUserName=\""
        }
      };

      proc.Start();
      var outputResult = proc.StandardOutput.ReadToEnd();
      var error = proc.StandardError.ReadToEnd();
      proc.WaitForExit();

      return outputResult;
    }

    private static void ParseActiveUserPage(string activeUserPageHtmlString)
    {
      // parse the HTML
      var doc = new HtmlDocument();
      doc.LoadHtml(activeUserPageHtmlString);

      var wizardPanelTableNode = doc.DocumentNode.SelectSingleNode("//table");
      var userListTableNode = wizardPanelTableNode.SelectSingleNode("//tr/td[2]/div[2]/div[1]/div[2]/div[2]/table[2]");
      foreach (HtmlNode rowNode in userListTableNode.SelectNodes("tr"))
      {
        var cellNodes = rowNode.SelectNodes("td");
        var lastActivityTime = ElementParser.GetLastActivityTime(cellNodes[7].InnerText);
        _activeUsers.Add(new User
        {
          DisplayName = cellNodes[5].InnerText,
          LastActivityTime = lastActivityTime,
          LogOffUri = ElementParser.GetLogUri(cellNodes[1]),
          Email = ElementParser.GetEmail(cellNodes[5])
        });
      }

      // sort by datetime asc
      _activeUsers = _activeUsers.OrderBy(x => x.LastActivityTime).ToList();

      // outputs to the console
      Console.WriteLine("Belows are the currect active users:");
      foreach (var user in _activeUsers)
      {
        Console.WriteLine($"{user.DisplayName}\t\t\t{user.LastActivityTime}");
      }
    }

    // logoff the user who is inactive for 20 minuts, and return the logoff user list
    private static List<User> LogoffUsersByInactiveDuration()
    {
      var logoffList = new List<User>();
      foreach (var user in _activeUsers)
      {
        var duration = (DateTime.Now - user.LastActivityTime).TotalSeconds;
        if (duration >= InactiveDurationThresholdInSeconds)
        {
          logoffList.Add(user);
        }
      }

      foreach (var userTobeLogoff in logoffList)
      {
        LogoffUser(userTobeLogoff);
        _activeUsers.Remove(userTobeLogoff);
      }

      return logoffList;
    }

    private static List<User> LogoffUsersByOrder()
    {
      var logoffList = new List<User>();
      if (_activeUsers.Count <= _maxOnlineUser) return logoffList;

      do
      {
        var userTobeLogOff = _activeUsers[0];
        logoffList.Add(userTobeLogOff);
        LogoffUser(userTobeLogOff);

        // remove the user from the list
        _activeUsers.RemoveAt(0);
      } while (_activeUsers.Count > _maxOnlineUser);
      return logoffList;
    }

    // need to logoff myself, or I will consume one seat after launch this tool ;-)
    private static void LogoffMyself()
    {
      foreach (var user in _activeUsers)
      {
        if (user.DisplayName.Equals("Carlos Liu"))
        {
          LogoffUser(user);
          break;
        }
      }
    }

    private static void LogoffUser(User userTobeLogoff)
    {
      var logoffProc = new Process
      {
        StartInfo =
        {
          FileName = "C:\\Program Files\\Collaborator Client\\ccollab.exe",
          UseShellExecute = false,
          RedirectStandardInput = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          CreateNoWindow = true,
          Arguments = $"admin wget \"{userTobeLogoff.LogOffUri}\""
        }
      };

      logoffProc.Start();
      var logoffOutputResult = logoffProc.StandardOutput.ReadToEnd();
      var logoffError = logoffProc.StandardError.ReadToEnd();
      logoffProc.WaitForExit();
    }


    private static void AddLogsForLogoffUsers(List<User> logoffUsers)
    {
      try
      {
        var logoffUsersList = new StringBuilder();
        foreach (var user in logoffUsers)
        {
          var item = $"Display Name: {user.DisplayName}, Last Activity Time: {user.LastActivityTime}";
          logoffUsersList.AppendLine(item);
          logoffUsersList.AppendLine();
        }
        WriteLogs(logoffUsersList.ToString());

        const string promptMessage = "The below accounts are logoff to release the license seats";
        var cmdLogText = promptMessage + Environment.NewLine + logoffUsersList;
        
        // outputs to console
        Console.WriteLine(cmdLogText);
      }
      catch (Exception ex)
      {
        Console.WriteLine("AddLogsForLogoffUsers: {0}", ex);
      }
    }
    
    private static void WriteLogs(string message)
    {
      var exeRunningPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      var logFile = Path.Combine(exeRunningPath, "LogoffUsers.log");

      using (var file = new StreamWriter(logFile, true))
      {
        file.Write(message);
      }
    }
  }


  class User
  {
    public string DisplayName { get; set; }
    public DateTime LastActivityTime { get; set; }
    public string LogOffUri { get; set; }
    public string Email { get; set; }
  }
}
