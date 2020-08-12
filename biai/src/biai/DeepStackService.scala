package biai

import cats.effect.{Blocker, ContextShift, IO}
import io.circe.generic.auto._
import org.http4s._
import org.http4s.circe._
import org.http4s.client.Client
import org.http4s.client.dsl.Http4sClientDsl
import org.http4s.headers.`Content-Type`
import org.http4s.multipart.{Multipart, Part}

import scala.concurrent.ExecutionContext

case class DeepStackObject(label: String, confidence: Float, y_min: Int, x_min: Int, y_max: Int, x_max: Int)
case class DeepStackResponse(success: Boolean, predictions: List[DeepStackObject])

trait DeepStackService {
  def detectObjects(image: Image): IO[DeepStackResponse]
}

class DeepStackServiceImpl(config: AppConfig, blocker: Blocker, httpClient: Client[IO], logger: Logger)(implicit
  ec: ExecutionContext,
  cs: ContextShift[IO]
) extends DeepStackService
    with Http4sClientDsl[IO] {

  implicit val objectDecoder: EntityDecoder[IO, DeepStackObject] = jsonOf[IO, DeepStackObject]
  implicit val responseDecoder: EntityDecoder[IO, DeepStackResponse] = jsonOf[IO, DeepStackResponse]

  override def detectObjects(image: Image): IO[DeepStackResponse] = {
    // TODO: handle this error
    val uri = Uri.unsafeFromString(config.deepStackEndpoint)

    val multipart: Multipart[IO] =
      Multipart[IO](
        Vector(Part.fileData("image", new java.io.File(image.fileName), blocker, `Content-Type`(MediaType.image.png)))
      )

    val request: IO[Request[IO]] = Method.POST(multipart, uri)

    for {
      _ <- logger.log("Before response")
      response <- httpClient.expect[DeepStackResponse](request)
      _ <- logger.log("After response")
    } yield response
  }
}
