import Paragraph from '@tiptap/extension-paragraph'
import { marked } from 'marked'

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

    // When token.tokens is empty (can happen after ordered lists due to TipTap's
    // custom list tokenizer affecting marked's inline queue), re-tokenize token.text
    // using marked.Lexer.lexInline to get proper inline tokens (strong, em, del, etc.)
    // so that inline styles are not rendered as raw markdown characters.
    const inlineTokens = tokens.length > 0
      ? tokens
      : token.text
        ? marked.Lexer.lexInline(token.text)
        : []

    const content = helpers.parseInline(inlineTokens)

    const hasExplicitEmptyParagraphMarker
      = inlineTokens.length === 1
        && inlineTokens[0]?.type === 'text'
        && (inlineTokens[0].raw === EMPTY_PARAGRAPH_MARKDOWN
          || inlineTokens[0].text === EMPTY_PARAGRAPH_MARKDOWN
          || inlineTokens[0].raw === NBSP_CHAR
          || inlineTokens[0].text === NBSP_CHAR
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
