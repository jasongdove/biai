import mill._, scalalib._

object biai extends ScalaModule {
    def scalaVersion = "2.13.2"
    
    def ivyDeps = Agg(
        ivy"org.typelevel::cats-effect:2.1.3",
        ivy"io.methvin::directory-watcher-better-files:0.9.10"
    )

    // def scalacOptions = Seq(
    //     "-Ymacro-annotations"
    // )
}