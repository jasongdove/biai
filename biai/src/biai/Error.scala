package biai

import better.files.File

sealed trait FatalError extends Throwable
sealed trait NonFatalError extends Throwable

case class CameraNotFound(file: File) extends NonFatalError {
  override def getMessage: String = s"Could not match camera for image at ${file.canonicalPath}"
}

case object UnableToPostImage extends NonFatalError {
  override def getMessage: String = "Unable to post image to DeepStack"
}

case object NoObjectsDetected extends NonFatalError {
  override def getMessage: String = "DeepStack did not detect any objects"
}

case object NoRelevantObjectsDetected extends NonFatalError {
  override def getMessage: String = "DeepStack did not detect any relevant objects"
}