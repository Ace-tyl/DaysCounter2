using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DaysCounter2.Utils
{
    public class Version(int major, int minor, int revision, int build)
    {
        public int major = major;
        public int minor = minor;
        public int revision = revision;
        public int build = build;

        public static Version FromString(string s)
        {
            List<string> splitted = s.Split('.').ToList();
            while (splitted.Count < 4)
            {
                splitted.Add("0");
            }
            int[] versionNumber = new int[4];
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    versionNumber[i] = int.Parse(splitted[i]);
                }
                catch
                {
                    versionNumber[i] = 0;
                }
            }
            return new Version(versionNumber[0], versionNumber[1], versionNumber[2], versionNumber[3]);
        }

        public bool EarlierThan(Version version)
        {
            if (major != version.major)
            {
                return major < version.major;
            }
            else if (minor != version.minor)
            {
                return minor < version.minor;
            }
            else if (revision != version.revision)
            {
                return revision < version.revision;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", major, minor, revision, build);
        }
    }

    internal class UpdateChecker
    {
        public Version currentVersion;
        public Version newestVersion = new(1, 0, 0, 0);
        public string releaseUrl = "";
        public bool checkFailed = false;

        public UpdateChecker()
        {
            var versionAttribute = (AssemblyFileVersionAttribute?)Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyFileVersionAttribute));
            if (versionAttribute != null)
            {
                currentVersion = Version.FromString(versionAttribute.Version);
            }
            else
            {
                // Fallback to the oldest version
                currentVersion = new Version(1, 0, 0, 0);
            }
        }

        public async Task GetNewestVersion()
        {
            try
            {
                // Get Newest Version
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "DaysCounter2UpdateChecker");

                    // Get release information from github API
                    string url = string.Format("https://api.github.com/repos/{0}/releases/latest", App.githubRepo);
                    string htmlContent = await client.GetStringAsync(url);

                    // Parse Json from release information
                    JsonDocument json = JsonDocument.Parse(htmlContent);

                    // Get newest version from json document
                    string tagVersion = json.RootElement.GetProperty("tag_name").GetString() ?? "1.0.0.0";

                    newestVersion = Version.FromString(tagVersion);
                    releaseUrl = json.RootElement.GetProperty("html_url").GetString() ?? string.Format("https://github.com/{0}/releases", App.githubRepo);
                }
            }
            catch
            {
                checkFailed = true;
            }
        }
    }
}
