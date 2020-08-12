package biai

import java.time.format.DateTimeFormatter
import java.time.{Instant, LocalDateTime, ZoneOffset}

import better.files.File

case class Image(fileName: String, cameraName: String, timestamp: Instant)

object Image {
  def load(fullPath: String): Option[Image] = {
    val fileName = File(fullPath).name
    fileName match {
      case s"$cameraName.$timestamp.$_" =>
        // TODO: failure to parse timestamp?
        Some(Image(fullPath, cameraName, parseUtc(timestamp)))
      case _ =>
        None
    }
  }

  private val formatter = DateTimeFormatter.ofPattern("yyyyMMdd_HHmmssSSS")

  private def parseUtc(timestamp: String): Instant =
    LocalDateTime.parse(timestamp, formatter).toInstant(ZoneOffset.UTC)
}