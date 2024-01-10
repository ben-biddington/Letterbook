using System.Reflection;
using ActivityPub.Types.AS;
using ActivityPub.Types.AS.Extended.Actor;
using ActivityPub.Types.AS.Extended.Object;
using ActivityPub.Types.Conversion;
using AutoMapper;
using Letterbook.Adapter.ActivityPub.Types;
using Letterbook.Core.Tests.Fakes;
using Letterbook.Core.Tests.Fixtures;
using Xunit.Abstractions;

namespace Letterbook.Adapter.ActivityPub.Test;

/// <summary>
/// Mapper tests are a little bit of a mess right now, but half the mappers will need to be rebuilt in the near future
/// anyway.
/// </summary>
public class MapperTests : IClassFixture<JsonLdSerializerFixture>
{
    private static string DataDir => Path.Join(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");

    public class MapFromModelTests : IClassFixture<JsonLdSerializerFixture>
    {
        private readonly ITestOutputHelper _output;

        private FakeProfile _fakeProfile;
        private Models.Profile _profile;
        private readonly IJsonLdSerializer _serializer;
#pragma warning disable CS8618
#pragma warning disable CS0649
        private static IMapper ModelMapper;
#pragma warning restore CS0649
#pragma warning restore CS8618

        public MapFromModelTests(ITestOutputHelper output, JsonLdSerializerFixture serializerFixture)
        {
            _output = output;
            _serializer = serializerFixture.JsonLdSerializer;

            _output.WriteLine($"Bogus Seed: {Init.WithSeed()}");
            _fakeProfile = new FakeProfile("letterbook.example");
            _profile = _fakeProfile.Generate();
        }

        [Fact(Skip = "Need ModelMapper")]
        public void MapProfileDefault()
        {
            var actual = ModelMapper.Map<PersonActorExtension>(_profile);

            Assert.Equal(_profile.Id.ToString(), actual.Id);
            Assert.Equal(_profile.Inbox.ToString(), actual.Inbox.Id);
            Assert.Equal(_profile.Outbox.ToString(), actual.Outbox.Id);
            Assert.Equal(_profile.Following.ToString(), actual.Following?.Id);
            Assert.Equal(_profile.Followers.ToString(), actual.Followers?.Id);
        }

        [Fact(Skip = "Need ModelMapper")]
        public void CanMapProfileDefaultSigningKey()
        {
            var expected = _profile.Keys.First().GetRsa().ExportSubjectPublicKeyInfoPem();

            var actual = ModelMapper.Map<PersonActorExtension>(_profile);

            Assert.Equal(actual?.PublicKey?.PublicKeyPem, expected);
            Assert.Equal(actual?.PublicKey?.Owner?.Value?.Id, _profile.Id.ToString());
            Assert.Equal(actual?.PublicKey?.Id, _profile.Keys.First().Id.ToString());
        }

        [Fact(Skip = "Need ModelMapper")]
        public void CanMapActorCore()
        {
            var actual = ModelMapper.Map<PersonActorExtension>(_profile);

            Assert.Equal(_profile.Id.ToString(), actual.Id);
            Assert.Equal(_profile.Inbox.ToString(), actual.Inbox.HRef);
            Assert.Equal(_profile.Outbox.ToString(), actual.Outbox.HRef);
            Assert.Equal(_profile.Following.ToString(), actual.Following?.HRef!);
            Assert.Equal(_profile.Followers.ToString(), actual.Followers?.HRef!);
            Assert.Equal(_profile.Handle, actual.PreferredUsername?.DefaultValue);
            Assert.Equal(_profile.DisplayName, actual.Name?.DefaultValue);
        }

        [Fact(Skip = "Need ModelMapper")]
        public void CanMapActorExtensionsPublicKey()
        {
            var expectedKey = _profile.Keys.First();
            var expectedPem = expectedKey.GetRsa().ExportSubjectPublicKeyInfoPem();
            var actual = ModelMapper.Map<PersonActorExtension>(_profile);

            Assert.Equal(expectedPem, actual.PublicKey?.PublicKeyPem);
            Assert.Equal(expectedKey.Id.ToString(), actual.PublicKey?.Id);
        }
    }

    public class MapFromAstTests : IClassFixture<JsonLdSerializerFixture>
    {
        private readonly ITestOutputHelper _output;
        private static IMapper AstMapper => new Mapper(Mappers.AstMapper.Default);
        private readonly IJsonLdSerializer _serializer;
        private NoteObject _simpleNote;

        public MapFromAstTests(ITestOutputHelper output, JsonLdSerializerFixture serializerFixture)
        {
            _output = output;
            _serializer = serializerFixture.JsonLdSerializer;

            _output.WriteLine($"Bogus Seed: {Init.WithSeed()}");
            
            _simpleNote = new NoteObject
            {
                Id = "https://note.example/note/1",
                Content = "<p>test content</p>",
                Source = new ASObject
                {
                    Content = "test content",
                    MediaType = "text"
                }
            };
            _simpleNote.AttributedTo.Add("https://note.example/actor/1");
        }

        [Fact]
        public void ValidConfig()
        {
            Mappers.AstMapper.Default.AssertConfigurationIsValid();
        }

        [Fact]
        public void CanMapLetterbookActor()
        {
            using var fs = new FileStream(Path.Join(DataDir, "LetterbookActor.json"), FileMode.Open);
            var actor = _serializer.Deserialize<PersonActorExtension>(fs)!;
            var mapped = AstMapper.Map<Models.Profile>(actor);

            Assert.NotNull(mapped);
        }

        [Fact]
        public void CanMapMastodonActor()
        {
            using var fs = new FileStream(Path.Join(DataDir, "Actor.json"), FileMode.Open);
            var actor = _serializer.Deserialize<PersonActorExtension>(fs)!;
            var mapped = AstMapper.Map<Models.Profile>(actor);

            Assert.NotNull(mapped);
        }

        [Fact]
        public void CanMapSimpleNote()
        {
            var mapped = AstMapper.Map<Models.Post>(_simpleNote);

            Assert.NotNull(mapped);
        }

        [Fact]
        public void NoItDoesnt()
        {
            string? s = null;
            Assert.Throws<ArgumentOutOfRangeException>(() => Extensions.NotNull(s));
            Assert.Throws<ArgumentOutOfRangeException>(() => Extensions.NotNull());
        }
    }
}