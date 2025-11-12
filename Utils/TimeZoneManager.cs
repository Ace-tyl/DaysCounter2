using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils
{
    public class TimeZoneData
    {
        public int delta { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public TimeZoneData(int delta, string name, string description)
        {
            this.delta = delta;
            this.name = name;
            this.description = description;
        }
    }

    public class TimeZoneManager
    {
        public static List<TimeZoneData> timeZoneData = new List<TimeZoneData>
        {
            new TimeZoneData(-720, "UTC-12:00", "Anywhere on Earth (AoE);\nBaker Island, Howland Island"),
            new TimeZoneData(-660, "UTC-11:00", "American Samoa, Jarvis Island, Kingman Reef, Midway Atoll, Niue, Palmyra Atoll"),
            new TimeZoneData(-600, "UTC-10:00", "Cook Islands, French Polynesia (most), Johnston Atoll, United States (Hawaii);\nUnited States (Andreanof Islands, Islands of Four Mountains, Near Islands, Rat Islands) without DST"),
            new TimeZoneData(-570, "UTC-09:30", "French Polynesia (Marquesas Islands)"),
            new TimeZoneData(-540, "UTC-09:00", "French Polynesia (Gambier Islands);\nUnited States (Alaska (most)) without DST;\nUnited States (Andreanof Islands, Islands of Four Mountains, Near Islands, Rat Islands) with DST"),
            new TimeZoneData(-480, "UTC-08:00", "Clipperton Island, Pitcairn Islands;\nCanada (British Columbia (most)), Mexico (Baja California), United States (California, Idaho (north), Nevada (most), Oregon (most), Washington) without DST;\nUnited States (Alaska (most)) with DST"),
            new TimeZoneData(-420, "UTC-07:00", "Canada (British Columbia (northeast), Yukon), Mexico (Baja California Sur, Nayarit (most), Sinaloa, Sonora), United States: Arizona (most);\nCanada (Alberta, British Columbia (southeast), Northwest Territories, Nunavut (west), Saskatchewan (Lloydminster area)), Mexico (Chihuahua (northwest border)), United States (Arizona (Navajo Nation), Colorado, Idaho (most), Kansas (west), Montana, Nebraska (west), New Mexico, Nevada (northeast border), North Dakota (southwest), Oregon (east), South Dakota (west), Texas (west), Utah, Wyoming) without DST;\nCanada (British Columbia (most)), Mexico (Baja California), United States (California, Idaho (north), Nevada (most), Oregon (most), Washington) with DST"),
            new TimeZoneData(-360, "UTC-06:00", "Belize, Canada (Saskatchewan (most)), Costa Rica, Ecuador (Galápagos), El Salvador, Guatemala, Honduras, Mexico (most), Nicaragua;\nCanada (Manitoba, Nunavut (central), Ontario (west)), Chile (Easter Island), Mexico (northeast border), United States (Alabama, Arkansas, Florida (northwest), Illinois, Indiana (northwest, southwest), Iowa, Kansas (most), Kentucky (west), Louisiana, Michigan (northwest border), Minnesota, Mississippi, Missouri, Nebraska (most), North Dakota (most), Oklahoma, South Dakota (most), Tennessee (most), Texas (most), Wisconsin) without DST;\nCanada (Alberta, British Columbia (southeast), Northwest Territories, Nunavut (west), Saskatchewan (Lloydminster area)), Mexico (Chihuahua (northwest border)), United States (Arizona (Navajo Nation), Colorado, Idaho (most), Kansas (west), Montana, Nebraska (west), New Mexico, Nevada (northeast border), North Dakota (southwest), Oregon (east), South Dakota (west), Texas (west), Utah, Wyoming) with DST"),
            new TimeZoneData(-300, "UTC-05:00", "Brazil (Acre, Amazonas (southwest)), Canada (Atikokan, Mishkeegogamang, Southampton Island), Cayman Islands, Colombia, Ecuador (most), Jamaica, Mexico (Quintana Roo), Navassa Island, Panama, Peru;\nBahamas, Canada (Nunavut (east), Ontario (most), Quebec (most)), Cuba, Haiti, Turks and Caicos Islands, United States (Connecticut, Delaware, District of Columbia, Florida (most), Georgia, Indiana (most), Kentucky (east), Maine, Maryland, Massachusetts, Michigan (most), New Hampshire, New Jersey, New York, North Carolina, Ohio, Pennsylvania, Rhode Island, South Carolina, Tennessee (east), Vermont, Virginia, West Virginia) without DST;\nCanada (Manitoba, Nunavut (central), Ontario (west)), Chile (Easter Island), Mexico (northeast border), United States (Alabama, Arkansas, Florida (northwest), Illinois, Indiana (northwest, southwest), Iowa, Kansas (most), Kentucky (west), Louisiana, Michigan (northwest border), Minnesota, Mississippi, Missouri, Nebraska (most), North Dakota (most), Oklahoma, South Dakota (most), Tennessee (most), Texas (most), Wisconsin) with DST"),
            new TimeZoneData(-240, "UTC-04:00", "Anguilla, Antigua and Barbuda, Aruba, Barbados, Bolivia, Brazil (Amazonas (most), Mato Grosso, Mato Grosso do Sul, Rondônia, Roraima), British Virgin Islands, Canada (Quebec (east)), Caribbean Netherlands, Curaçao, Dominica, Dominican Republic, Grenada, Guadeloupe, Guyana, Martinique, Montserrat, Puerto Rico, Saint Barthélemy, Saint Kitts and Nevis, Saint Lucia, Saint Martin, Saint Vincent and the Grenadines, Sint Maarten, Trinidad and Tobago, U.S. Virgin Islands, Venezuela;\nBermuda, Canada (Labrador (most), New Brunswick, Nova Scotia, Prince Edward Island), Chile (most), Greenland (Pituffik Space Base) without DST;\nBahamas, Canada (Nunavut (east), Ontario (most), Quebec (most)), Cuba, Haiti, Turks and Caicos Islands, United States (Connecticut, Delaware, District of Columbia, Florida (most), Georgia, Indiana (most), Kentucky (east), Maine, Maryland, Massachusetts, Michigan (most), New Hampshire, New Jersey, New York, North Carolina, Ohio, Pennsylvania, Rhode Island, South Carolina, Tennessee (east), Vermont, Virginia, West Virginia) with DST"),
            new TimeZoneData(-210, "UTC-03:30", "Canada (Newfoundland, Labrador (southeast)) without DST"),
            new TimeZoneData(-180, "UTC-03:00", "Argentina, Brazil (most), Chile (Aysén, Magallanes), Falkland Islands, French Guiana, Paraguay, Suriname, Uruguay;\nSaint Pierre and Miquelon without DST;\nBermuda, Canada (Labrador (most), New Brunswick, Nova Scotia, Prince Edward Island), Chile (most), Greenland (Pituffik Space Base) with DST"),
            new TimeZoneData(-150, "UTC-02:30", "Canada (Newfoundland, Labrador (southeast)) with DST"),
            new TimeZoneData(-120, "UTC-02:00", "Brazil (Fernando de Noronha), South Georgia and the South Sandwich Islands;\nGreenland (most) without DST;\nSaint Pierre and Miquelon with DST"),
            new TimeZoneData(- 60, "UTC-01:00", "Cape Verde;\nPortugal (Azores) without DST;\nGreenland (most) with DST"),
            new TimeZoneData(+  0, "UTC+00:00", "Burkina Faso, Gambia, Ghana, Greenland (National Park (east coast)), Guinea, Guinea-Bissau, Iceland, Ivory Coast, Liberia, Mali, Mauritania, Saint Helena, Ascension and Tristan da Cunha, Senegal, Sierra Leone, São Tomé and Príncipe, Togo;\nFaroe Islands, Guernsey, Ireland, Isle of Man, Jersey, Portugal (most), Spain (Canary Islands), United Kingdom without DST;\nPortugal (Azores) with DST"),
            new TimeZoneData(+ 48, "UTC+00:48", "Kindom of Elleore without DST"),
            new TimeZoneData(+ 60, "UTC+01:00", "Algeria, Angola, Benin, Cameroon, Central African Republic, Chad, Congo, DR Congo (Équateur, Kinshasa, Kongo Central, Kwango, Kwilu, Mai-Ndombe, Mongala, Nord-Ubangi, Sud-Ubangi, Tshuapa), Equatorial Guinea, Gabon, Morocco, Niger, Nigeria, Tunisia;\nAlbania, Andorra, Austria, Belgium, Bosnia and Herzegovina, Croatia, Czech Republic, Denmark, France (metropolitan), Germany, Gibraltar, Hungary, Italy, Liechtenstein, Luxembourg, Malta, Monaco, Montenegro, Netherlands (European), North Macedonia, Norway, Poland, San Marino, Serbia, Slovakia, Slovenia, Spain (most), Sweden, Switzerland, Vatican City without DST;\nFaroe Islands, Guernsey, Ireland, Isle of Man, Jersey, Portugal (most), Spain (Canary Islands), United Kingdom with DST"),
            new TimeZoneData(+108, "UTC+01:48", "Kindom of Elleore with DST"),
            new TimeZoneData(+120, "UTC+02:00", "Botswana, Burundi, DR Congo (most), Eswatini, Lesotho, Libya, Malawi, Mozambique, Namibia, Russia (Kaliningrad), Rwanda, South Africa (most), South Sudan, Sudan, Zambia, Zimbabwe;\nAkrotiri and Dhekelia, Bulgaria, Cyprus, Egypt, Estonia, Finland, Greece, Israel, Latvia, Lebanon, Lithuania, Moldova, Palestine, Romania, Ukraine (most) without DST;\nAlbania, Andorra, Austria, Belgium, Bosnia and Herzegovina, Croatia, Czech Republic, Denmark, France (metropolitan), Germany, Gibraltar, Hungary, Italy, Liechtenstein, Luxembourg, Malta, Monaco, Montenegro, Netherlands (European), North Macedonia, Norway, Poland, San Marino, Serbia, Slovakia, Slovenia, Spain (most), Sweden, Switzerland, Vatican City with DST"),
            new TimeZoneData(+180, "UTC+03:00", "Bahrain, Belarus, Comoros, Djibouti, Eritrea, Ethiopia, French Southern and Antarctic Lands (Scattered Islands), Iraq, Jordan, Kenya, Kuwait, Madagascar, Mayotte, Qatar, Russia (most of European part), Saudi Arabia, Somalia, South Africa (Prince Edward Islands), Syria, Tanzania, Türkiye, Uganda, Ukraine (occupied territories), Yemen;\nAkrotiri and Dhekelia, Bulgaria, Cyprus, Egypt, Estonia, Finland, Greece, Israel, Latvia, Lebanon, Lithuania, Moldova, Palestine, Romania, Ukraine (most) with DST"),
            new TimeZoneData(+210, "UTC+03:30", "Iran"),
            new TimeZoneData(+240, "UTC+04:00", "Armenia, Azerbaijan, French Southern and Antarctic Lands (Crozet Islands), Georgia, Mauritius, Oman, Russia (Astrakhan, Samara, Saratov, Udmurtia, Ulyanovsk), Réunion, Seychelles, United Arab Emirates"),
            new TimeZoneData(+270, "UTC+04:30", "Afghanistan"),
            new TimeZoneData(+300, "UTC+05:00", "French Southern and Antarctic Lands (Kerguelen Islands, Saint Paul Island, Amsterdam Island), Heard Island and McDonald Islands, Kazakhstan, Maldives, Pakistan, Russia (Bashkortostan, Chelyabinsk, Khanty-Mansi, Kurgan, Orenburg, Perm, Sverdlovsk, Tyumen, Yamalia), Tajikistan, Turkmenistan, Uzbekistan"),
            new TimeZoneData(+330, "UTC+05:30", "India, Sri Lanka"),
            new TimeZoneData(+345, "UTC+05:45", "Nepal"),
            new TimeZoneData(+360, "UTC+06:00", "Bangladesh, Bhutan, British Indian Ocean Territory, Kyrgyzstan, Russia (Omsk)"),
            new TimeZoneData(+390, "UTC+06:30", "Cocos Islands, Myanmar"),
            new TimeZoneData(+420, "UTC+07:00", "Cambodia, Christmas Island, Indonesia (Sumatra, Java, West Kalimantan, Central Kalimantan), Laos, Mongolia (Bayan-Ölgii, Khovd, Uvs), Russia (Altai Krai, Altai Republic, Kemerovo, Khakassia, Krasnoyarsk, Novosibirsk, Tomsk, Tuva), Thailand, Vietnam"),
            new TimeZoneData(+480, "UTC+08:00", "Australia (Western Australia), Brunei, China, Indonesia (South Kalimantan, East Kalimantan, North Kalimantan, Sulawesi, Lesser Sunda Islands), Mongolia (most), Malaysia, Philippines, Russia (Buryatia, Irkutsk), Singapore"),
            new TimeZoneData(+525, "UTC+08:45", "Australia (Eucla)"),
            new TimeZoneData(+540, "UTC+09:00", "Indonesia (Maluku Islands, Western New Guinea), Japan, North Korea, Palau, Russia (Amur, Sakha (most), Zabaykalsky), South Korea, Timor-Leste"),
            new TimeZoneData(+570, "UTC+09:30", "Australia (Northern Territory);\nAustralia (South Australia, Yancowinna County) without DST"),
            new TimeZoneData(+600, "UTC+10:00", "Australia (Queensland), Guam, Micronesia (Chuuk, Yap), Northern Mariana Islands, Papua New Guinea (most), Russia (Jewish, Khabarovsk, Primorsky, Sakha (central-east));\nAustralia (Australian Capital Territory, Jervis Bay Territory, New South Wales (most), Tasmania, Victoria) without DST"),
            new TimeZoneData(+630, "UTC+10:30", "Australia (Lord Howe Island) without DST;\nAustralia (South Australia, Yancowinna County) with DST"),
            new TimeZoneData(+660, "UTC+11:00", "Micronesia (Kosrae, Pohnpei), New Caledonia, Papua New Guinea (Bougainville), Russia (Magadan, Sakha (east), Sakhalin), Solomon Islands, Vanuatu;\nNorfolk Island without DST;\nAustralia (Australian Capital Territory, Jervis Bay Territory, Lord Howe Island, New South Wales (most), Tasmania, Victoria) with DST"),
            new TimeZoneData(+720, "UTC+12:00", "Fiji, Kiribati (Gilbert Islands), Marshall Islands, Nauru, Russia (Chukotka, Kamchatka), Tuvalu, Wake Island, Wallis and Futuna;\nNew Zealand (most) without DST;\nNorfolk Island with DST"),
            new TimeZoneData(+765, "UTC+12:45", "New Zealand (Chatham Islands) without DST"),
            new TimeZoneData(+780, "UTC+13:00", "Kiribati (Phoenix Islands), Samoa, Tokelau, Tonga;\nNew Zealand (most) with DST"),
            new TimeZoneData(+825, "UTC+13:45", "New Zealand (Chatham Islands) with DST"),
            new TimeZoneData(+840, "UTC+14:00", "Kiribati (Line Islands)"),
        };
    }
}
