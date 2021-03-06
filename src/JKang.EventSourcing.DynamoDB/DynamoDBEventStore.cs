﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using JKang.EventSourcing.Domain;
using JKang.EventSourcing.Events;
using JKang.EventSourcing.Serialization.Json;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.EventSourcing.Persistence.DynamoDB
{
    internal class DynamoDBEventStore<TAggregate, TAggregateKey> : IEventStore<TAggregate, TAggregateKey>
        where TAggregate : IAggregate<TAggregateKey>
    {
        private readonly IJsonObjectSerializer _serializer;
        private readonly Table _table;

        public DynamoDBEventStore(
            IJsonObjectSerializer serializer,
            IOptionsMonitor<DynamoDBEventStoreOptions> monitor,
            IAmazonDynamoDB client)
        {
            _serializer = serializer;
            DynamoDBEventStoreOptions options = monitor.Get(typeof(TAggregate).FullName);
            if (options.UseLocalDB)
            {
                client = options.CreateLocalDBClient();
            }
            _table = Table.LoadTable(client, options.TableName);
        }

        public async Task AddEventAsync(
            IAggregateEvent<TAggregateKey> @event,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = _serializer.Serialize(@event);
            var item = Document.FromJson(json);
            Document re = await _table.PutItemAsync(item, cancellationToken);
        }

        public Task<TAggregateKey[]> GetAggregateIdsAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(new TAggregateKey[0]);
        }

        public async Task<IAggregateEvent<TAggregateKey>[]> GetEventsAsync(
            TAggregateKey aggregateId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Search search = _table.Query(aggregateId as dynamic, new QueryFilter());

            var events = new List<IAggregateEvent<TAggregateKey>>();
            do
            {
                List<Document> documents = await search.GetNextSetAsync(cancellationToken);
                foreach (Document document in documents)
                {
                    string json = document.ToJson();
                    IAggregateEvent<TAggregateKey> @event = _serializer.Deserialize<IAggregateEvent<TAggregateKey>>(json);
                    events.Add(@event);
                }
            } while (!search.IsDone);

            return events.OrderBy(x => x.AggregateVersion).ToArray();
        }
    }
}