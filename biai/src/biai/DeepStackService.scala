package biai

import cats.effect.{Blocker, ContextShift, IO}
import cats.implicits._
import io.circe.generic.auto._
import org.http4s._
import org.http4s.circe._
import org.http4s.client.Client
import org.http4s.client.dsl.Http4sClientDsl
import org.http4s.headers.`Content-Type`
import org.http4s.multipart.{Multipart, Part}

case class DeepStackObject(label: String, confidence: Float, y_min: Int, x_min: Int, y_max: Int, x_max: Int)
case class DeepStackResponse(success: Boolean, predictions: List[DeepStackObject])

trait DeepStackService {
  def detectObjects(image: Image): IO[DeepStackResponse]
}

class DeepStackServiceImpl(config: AppConfig, blocker: Blocker, httpClient: Client[IO], logger: Logger)(implicit
  cs: ContextShift[IO]
) extends DeepStackService
    with Http4sClientDsl[IO] {

  implicit val objectDecoder: EntityDecoder[IO, DeepStackObject] = jsonOf[IO, DeepStackObject]
  implicit val responseDecoder: EntityDecoder[IO, DeepStackResponse] = jsonOf[IO, DeepStackResponse]

  override def detectObjects(image: Image): IO[DeepStackResponse] = {
    val multipart: Multipart[IO] =
      Multipart[IO](
        Vector(Part.fileData[IO]("image", image.file.toJava, blocker, `Content-Type`(MediaType.image.png)))
      )

    for {
      uri <- Uri.fromString(config.deepStackEndpoint).liftTo[IO]
      request <- Method.POST(multipart, uri).map(_.withHeaders(multipart.headers))
      response <- httpClient.expect[DeepStackResponse](request)
    } yield response
  }
}
