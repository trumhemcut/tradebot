﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using tradebot.core;

namespace tradebot.api.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private ICapPublisher _capPublisher;
        public ValuesController(ICapPublisher capPublisher)
        {
            this._capPublisher = capPublisher;
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _capPublisher.Publish("hello", new TestMessage() { Message = "Hello World" });
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
