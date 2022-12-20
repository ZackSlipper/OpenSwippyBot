using System.Collections.Generic;

namespace SwippyBot
{
    public static class AbbreviationTimeZones
    {
        static Dictionary<string, int> abbreviations = new Dictionary<string, int>()
        {
            {"a", 1 },
            {"acdt", 11 },
            {"acst", 10 },
            {"act", 10 },
            {"acwst", 9 },
            {"adt", 4 },
            {"aedt", 11 },
            {"aest", 10 },
            {"aet", 10 },
            {"aft", 5 },
            {"akdt", -8 },
            {"akst", -9 },
            {"almt", 6 },
            {"amst", 5 },
            {"amt", 4 },
            {"anast", 12 },
            {"anat", 12 },
            {"aqtt", 5 },
            {"art", -3 },
            {"ast", -4 },
            {"at", -4 },
            {"awdt", 9 },
            {"awst", 8 },
            {"azost", 0 },
            {"azot", -1 },
            {"azst", 5 },
            {"azt", 4 },
            {"aoe", -12 },
            {"b", 2 },
            {"bnt", 8 },
            {"bot", -4 },
            {"brst", -2 },
            {"brt", -3 },
            {"bst", 6 },
            {"bdt", 1 },
            {"bdst", 1 },
            {"btt", 6 },
            {"c", 3 },
            {"cast", 8 },
            {"cat", 2 },
            {"cct", 7 },
            {"cdt", -5 },
            {"cdst", -5 },
            {"nacdt", -5 },
            {"hac", -5 },
            {"cest", 2 },
            {"cedt", 2 },
            {"ecst", 2 },
            {"mesz", 2 },
            {"cet", 1 },
            {"ect", 1 },
            {"mez", 1 },
            {"chadt", 14 },
            {"chast", 13 },
            {"chost", 9 },
            {"chodt", 9 },
            {"chodst", 9 },
            {"chot", 8 },
            {"chut", 10 },
            {"cidst", -4 },
            {"cist", -5 },
            {"cit", -5 },
            {"ckt", -10 },
            {"clst", -3 },
            {"cldt", -3 },
            {"clt", -4 },
            {"cot", -5 },
            {"cst", -6 },
            {"ct", -6 },
            {"nacst", -6 },
            {"hnc", -6 },
            {"cvt", -1 },
            {"cxt", 7 },
            {"chst", 10 },
            {"d", 4 },
            {"davt", 7 },
            {"ddut", 10 },
            {"e", 5 },
            {"easst", -5 },
            {"eadt", -5 },
            {"east", -6 },
            {"eat", 3 },
            {"edst", -4 },
            {"naedt", -4 },
            {"hae", -4 },
            {"edt", -4 },
            {"eet", 2},
            {"eest", 3 },
            {"eedt", 3 },
            {"oesz", 3 },
            {"oez", 2 },
            {"egst", 0 },
            {"egt", -1 },
            {"est", -5 },
            {"et", -5 },
            {"naest", -5 },
            {"hne", -5 },
            {"f", 6 },
            {"fet", 3 },
            {"fjst", 13 },
            {"fjdt", 13 },
            {"fjt", 12 },
            {"fkst", -3 },
            {"fkdt", -3 },
            {"fkt", -4 },
            {"fnt", -2 },
            {"g", 7 },
            {"galt", -6 },
            {"gamt", -9 },
            {"get", 4 },
            {"gft", -3 },
            {"gilt", 12 },
            {"gmt", 0 },
            {"gst", 4 },
            {"gyt", -4 },
            {"h", 8 },
            {"hdt", -9 },
            {"hadt", -9 },
            {"hkt", 8 },
            {"hovst", 8 },
            {"hovdt", 8 },
            {"hovdst", 8 },
            {"hovt", 7 },
            {"hst", -10 },
            {"hast", -10 },
            {"i", 9 },
            {"ict", 7 },
            {"idt", 3 },
            {"iot", 6 },
            {"irdt", 5 },
            {"irkst", 9 },
            {"irkt", 8 },
            {"irst", 4 },
            {"ist", 6 },
            {"it", 6 },
            {"jst", 9 },
            {"k", 10 },
            {"kgt", 6 },
            {"kost", 11 },
            {"krast", 8 },
            {"krat", 7 },
            {"kst", 9 },
            {"kt", 9 },
            {"kuyt", 4 },
            {"samst", 4 },
            {"l", 11 },
            {"lhdt", 11 },
            {"lhst", 11 },
            {"lint", 14 },
            {"m", 12 },
            {"magst", 12 },
            {"magt", 11 },
            {"mart", -10 },
            {"mawt", 5 },
            {"mdt", 6 },
            {"mdst", 6 },
            {"namdt", 6 },
            {"har", 6 },
            {"mht", 12 },
            {"mmt", 7 },
            {"msd", 4 },
            {"msk", 2 },
            {"mck", 3 },
            {"mst", -7 },
            {"mt", -7 },
            {"namst", -7 },
            {"hnr", -7 },
            {"mut", 4 },
            {"mvt", 5 },
            {"myt", 8 },
            {"n", -1 },
            {"nct", 11 },
            {"ndt", -3 },
            {"hat", -3 },
            {"nfdt", 12 },
            {"nft", 11 },
            {"novst", 7 },
            {"omsst", 7 },
            {"novt", 7 },
            {"npt", 6 },
            {"nrt", 12 },
            {"nst", -4 },
            {"hnt", -4 },
            {"nut", -11 },
            {"nzdt", 13 },
            {"nzst", 12 },
            {"o", -2 },
            {"omst", 6 },
            {"orat", 5 },
            {"p", -3 },
            {"pdt", -7 },
            {"pdst", -7 },
            {"napdt", -7 },
            {"hap", -7 },
            {"pet", -5 },
            {"petst", 12 },
            {"pett", 12 },
            {"pgt", 10 },
            {"phot", 13 },
            {"pht", 8 },
            {"pkt", 5 },
            {"pmdt", -2 },
            {"pmst", -3 },
            {"pont", 11 },
            {"pst",-8 },
            {"pt", -8 },
            {"napst", -8 },
            {"hnp", -8 },
            {"pwt", 9 },
            {"pyst", -3 },
            {"pyt", -4 },
            {"q", -4 },
            {"qyzt", 6 },
            {"r", -5 },
            {"ret", 4 },
            {"rott", -3 },
            {"s", -6 },
            {"sakt", 11 },
            {"samt", 4 },
            {"sast", 2 },
            {"sbt", 11 },
            {"sct", 4 },
            {"sgt", 8 },
            {"sret", 11 },
            {"srt", -3 },
            {"sst", -11 },
            {"syot", +3 },
            {"t", -7 },
            {"taht", -10 },
            {"tft", 5 },
            {"kit", 5 },
            {"tjt", 5 },
            {"tkt", 13 },
            {"tlt", 9 },
            {"tmt", 5 },
            {"tost", 14 },
            {"tot", 13 },
            {"trt", 3 },
            {"tvt", 12 },
            {"u", -8 },
            {"ulast", 9 },
            {"ulat", 8 },
            {"utc", 0 },
            {"uyst", -2 },
            {"uyt", -3 },
            {"uzt", +5 },
            {"v", -9 },
            {"vet", -4 },
            {"hlv", -4 },
            {"vlast", 11 },
            {"vlat", 10 },
            {"vost", 6 },
            {"vut", 11 },
            {"efate", 11 },
            {"w", -10 },
            {"wakt", 12 },
            {"warst", -3 },
            {"wast", 2 },
            {"wat", 1 },
            {"west", 1 },
            {"wedt", 1 },
            {"wesz", 1 },
            {"wet", 0 },
            {"wez", 0 },
            {"wft", 12 },
            {"wgst", -2 },
            {"wgt", -3 },
            {"wib", 7 },
            {"wit", 9 },
            {"wita", 8 },
            {"st", 13 },
            {"wst", 1 },
            {"wt", 0 },
            {"x", -11 },
            {"y", -12 },
            {"yakst", 10 },
            {"yakt", 9 },
            {"yapt", 10 },
            {"yekst", 6 },
            {"yekt", 5 },
            {"z", 0 }
        };

        public static int? Get(string abbreviation)
        {
            abbreviation = abbreviation.ToLower();
            if (abbreviation.Contains(abbreviation))
                return abbreviations[abbreviation];
            return null;
        }
    }
}