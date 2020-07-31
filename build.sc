import mill._, scalalib._

object biai extends ScalaModule {
    def scalaVersion = "2.13.3"
    
    def ivyDeps = Agg(
        ivy"org.typelevel::cats-effect:2.1.3",
        ivy"io.methvin::directory-watcher-better-files:0.9.10",
        ivy"com.github.pureconfig::pureconfig:0.13.0"
    )

    // def scalacOptions = Seq(
    //     "-Ymacro-annotations"
    // )
}