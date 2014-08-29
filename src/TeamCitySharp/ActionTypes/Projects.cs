﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using EasyHttp.Http;
using JsonFx.Json;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;
using TeamCitySharp.Connection;
using TeamCitySharp.DomainEntities;
using TeamCitySharp.Locators;

namespace TeamCitySharp.ActionTypes
{
    internal class Projects : IProjects
    {
        private readonly ITeamCityCaller _caller;

        internal Projects(ITeamCityCaller caller)
        {
            _caller = caller;
        }

        public List<Project> All()
        {
            var projectWrapper = _caller.Get<ProjectWrapper>("/app/rest/projects");

            return projectWrapper.Project;
        }

        public Project ByName(string projectLocatorName)
        {
            var project = _caller.GetFormat<Project>("/app/rest/projects/name:{0}", projectLocatorName);

            return project;
        }

        public Project ById(string projectLocatorId)
        {
            var project = _caller.GetFormat<Project>("/app/rest/projects/id:{0}", projectLocatorId);

            return project;
        }

        public Project Details(Project project)
        {
            return ById(project.Id);
        }

        public Project Create(string projectName)
        {
          return _caller.Post<Project>(projectName, HttpContentTypes.TextPlain, "/app/rest/projects/", HttpContentTypes.ApplicationJson);
        }

        public Project Create(string projectName, string sourceId, string projectId = "")
        {
            var id = projectId == "" ? GenerateID(projectName) : projectId;
            var xmlData = String.Format( "<newProjectDescription name='{0}' id='{1}'><parentProject locator='id:{2}'/></newProjectDescription>", projectName, id , sourceId);
            var response = _caller.Post(xmlData, HttpContentTypes.ApplicationXml, "/app/rest/projects", HttpContentTypes.ApplicationJson);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var reader =
                new JsonReader( new DataReaderSettings(new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-")));
                var project = reader.Read<Project>(response.RawText);
                return project;
            }
            return new Project();
        }

        public Project Move(string projectId, string destinationId)
        {
            var xmlData = String.Format("<project-ref locator='id:{0}'/>", destinationId);
            var url = String.Format("/app/rest/projects/id:{0}/parentProject",  projectId);
            var response = _caller.Put(xmlData, HttpContentTypes.ApplicationXml, url, HttpContentTypes.ApplicationJson);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                 var reader = new JsonReader( new DataReaderSettings(new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-")));
                var project = reader.Read<Project>(response.RawText);
                return project;
            }
            return new Project();
        }

        internal HttpResponse CopyProject(string projectid, string projectName, string newProjectId)
        {
          var xmlData = String.Format("<newProjectDescription name='{0}' id='{1}' copyAllAssociatedSettings='true'><sourceProject locator='id:{2}'/></newProjectDescription>", projectName, newProjectId, projectid);
            var response = _caller.Post(xmlData, HttpContentTypes.ApplicationXml, "/app/rest/projects", HttpContentTypes.ApplicationJson);
            return response;
        }
        public Project Copy(string projectid, string projectName, string newProjectId)
        {
          var response = CopyProject(projectid, projectName, newProjectId);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var reader = new JsonReader(new DataReaderSettings(new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-")));
                var project = reader.Read<Project>(response.RawText);
                return project;
            }
            return new Project();
        }
        public void Delete(string projectName)
        {
            _caller.DeleteFormat("/app/rest/projects/name:{0}", projectName);
        }

        public void DeleteProjectParameter(string projectName, string parameterName)
        {
            _caller.DeleteFormat("/app/rest/projects/name:{0}/parameters/{1}", projectName, parameterName);
        }

        public void SetProjectParameter(string projectName, string settingName, string settingValue)
        {
            _caller.PutFormat(settingValue, "/app/rest/projects/name:{0}/parameters/{1}", projectName, settingName);
        }
        public string GenerateID(string projectName)
        {
            projectName = Regex.Replace(projectName, @"[^\p{L}\p{N}]+", "");
            return projectName;
        }
        public bool ModifParameters(string buildTypeId, string param, string value)
        {
            var url = String.Format("/app/rest/projects/id:{0}/parameters/{1}", buildTypeId, param);

            var response = _caller.Put(value, HttpContentTypes.TextPlain, url, string.Empty);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public bool ModifSettings(string projectId, string setting, string value)
        {
            var url = String.Format("/app/rest/projects/{0}/{1}", projectId, setting);
            var response = _caller.Put(value, HttpContentTypes.TextPlain, url, string.Empty);
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}