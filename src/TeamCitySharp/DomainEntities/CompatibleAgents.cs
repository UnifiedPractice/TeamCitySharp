﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeamCitySharp.DomainEntities
{
  public class CompatibleAgents
  {
    [JsonProperty("href")]
    public string Href { get; set; }
  }
}