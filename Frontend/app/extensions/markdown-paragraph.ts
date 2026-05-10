import Paragraph from '@tiptap/extension-paragraph'

const EMPTY_PARAGRAPH_MARKDOWN = '&nbsp;'
const NBSP_CHAR = '\u00A0'

export const MarkdownParagraphExtension = Paragraph.extend({
  priority: 900,

  parseMarkdown: (token, helpers) => {
    const tokens = token.tokens || []
    const firstToken = tokens[0]

    if (tokens.length === 1 && firstToken?.type === 'image') {
      return helpers.parseChildren([firstToken])
    }

    const content = tokens.length > 0
      ? helpers.parseInline(tokens)
      : token.text
        ? helpers.parseInline([
            {
              type: 'text',
              raw: token.text,
              text: token.text
            }
          ])
        : []

    const hasExplicitEmptyParagraphMarker
      = tokens.length === 1
        && firstToken?.type === 'text'
        && (firstToken.raw === EMPTY_PARAGRAPH_MARKDOWN
          || firstToken.text === EMPTY_PARAGRAPH_MARKDOWN
          || firstToken.raw === NBSP_CHAR
          || firstToken.text === NBSP_CHAR
          || token.text === EMPTY_PARAGRAPH_MARKDOWN
          || token.text === NBSP_CHAR)

    if (
      hasExplicitEmptyParagraphMarker
      && content.length === 1
      && content[0]?.type === 'text'
      && (content[0].text === EMPTY_PARAGRAPH_MARKDOWN || content[0].text === NBSP_CHAR)
    ) {
      return helpers.createNode('paragraph', undefined, [])
    }

    return helpers.createNode('paragraph', undefined, content)
  }
})
