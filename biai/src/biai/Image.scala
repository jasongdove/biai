package biai

import java.time.format.DateTimeFormatter
import java.time.{Instant, LocalDateTime, ZoneOffset}

import better.files.File
import cats.effect.IO

case class Image(file: File, cameraName: String, timestamp: Instant)

object Image {
  def load(file: File): IO[Image] = {
    val fileName = file.name
    fileName match {
      case s"$cameraName.$timestamp.$_" =>
        for {
          utc <- parseUtc(timestamp)
        } yield Image(file, cameraName, utc)
      case other =>
        IO.raiseError(new Exception(s"Unable to parse file $other"))
    }
  }

  private val formatter = DateTimeFormatter.ofPattern("yyyyMMdd_HHmmssSSS")

  private def parseUtc(timestamp: String): IO[Instant] =
    IO(LocalDateTime.parse(timestamp, formatter).toInstant(ZoneOffset.UTC))
}
