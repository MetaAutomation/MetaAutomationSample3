<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <!--Note 1: (Atomic Check aspects reflected here: Separate Presentation) -->
  <!--This is the XSL spreadsheet that gives presentation to the pure data of XML. This depends on some -->
  <!-- XML formatting, but there is no XSD or other schema for the data. For a larger project or with a larger -->
  <!-- number of stylesheets, an XSD would be helpful. -->
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <title>The data for one Check result</title>
      </head>
      <body>
        <h2>
          Results for a run of Check <h1>
            <xsl:value-of select="/CheckRunArtifact/CheckRunData/DataElement[@Name='CheckName']/@Value"/>

          </h1>
        </h2>
        <h2>
          Result:
          <xsl:choose>
            <xsl:when test="/CheckRunArtifact/CheckFailData/DataElement">
              <b style="color:red">FAIL</b>
            </xsl:when>
            <xsl:otherwise>
              <b style="color:black">PASS</b>
            </xsl:otherwise>
          </xsl:choose>
        </h2>
        <table style="color:black">
          <xsl:for-each select="/CheckRunArtifact/CheckRunData/DataElement">
            <xsl:choose>
              <xsl:when test="@Name='CheckName'"/>
              <xsl:otherwise>
                <tr>
                  <td>
                    <xsl:value-of select="@Name"/>
                  </td>
                  <td>=</td>
                  <td>
                    <xsl:value-of select="@Value"/>
                  </td>
                </tr>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </table>

        <ul style="color:blue">
          <xsl:apply-templates select="/CheckRunArtifact/CompleteCheckStepInfo/CheckStep"/>
        </ul>

        <ul style="color:red">
          <xsl:apply-templates select="/CheckRunArtifact/CheckFailData/DataElement"/>
        </ul>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="CheckStep">
    <li>
      Step:
      <xsl:value-of select="@Name"/>

      <xsl:element name="b">
        <xsl:choose>
          <xsl:when test="@Value='Fail'">
            <xsl:attribute name="style">
              color:red
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="@Value='Blocked'">
            <xsl:attribute name="style">
              color:black
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="style">
              color:green
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@Value"/>
      </xsl:element>

      <xsl:if test="@msTimeElapsed">
        elapsed time
        <xsl:value-of select="@msTimeElapsed"/>
        ms
      </xsl:if>
      <xsl:if test="@msTimeLimit">
        of
        <xsl:value-of select="@msTimeLimit"/>
        ms limit
      </xsl:if>

      <ul>
        <xsl:apply-templates select="CheckStep"/>
      </ul>
    </li>
  </xsl:template>

  <xsl:template match="DataElement">
    <li>
      <xsl:value-of select="@Name"/>
      <xsl:choose>
        <xsl:when test="@Value">
          =
          <xsl:value-of select="@Value"/>
        </xsl:when>
        <xsl:otherwise>
          <ul>
            <xsl:apply-templates select="DataElement"/>
          </ul>
        </xsl:otherwise>
      </xsl:choose>
    </li>
  </xsl:template>

</xsl:stylesheet>
